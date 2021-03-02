using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QandA.Data;
using QandA.Data.Models;
using Microsoft.AspNetCore.SignalR;
using QandA.Hubs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text.Json;
namespace QandA
{
  
    [Route("api/[controller]")]
    [ApiController]
    public class QuestionsController : ControllerBase
    {
        private readonly IDataRepository _dataRepository;
        private readonly IHubContext<QuestionsHub>_questionHubContext;
        private readonly IHttpClientFactory _clientFactory;
        private readonly string _authOUserInfo;
       
        //We are going to inject the context of the hub into the API controller using dependency injection:
        public QuestionsController(IDataRepository dataRepository, IHubContext<QuestionsHub> questionHubContext, IHttpClientFactory clientFactory,IConfiguration configuration)
        {
            _dataRepository = dataRepository;
            _questionHubContext = questionHubContext;
            _clientFactory = clientFactory;
            _authOUserInfo = $"{configuration["Auth0:Authority"]}userinfo";
          

        }

        [HttpGet]
        public IEnumerable<QuestionGetManyResponse> GetQuestions(string search,bool includeAnswers)
        {
            if (string.IsNullOrEmpty(search))
            {
                if (includeAnswers)
                {
                    return _dataRepository.GetQuestionsWithAnswers();
                }
                else
                {
                    return _dataRepository.GetQuestions();
                }
                // If there is no search value, we get and return all the questions as we
                //did before, but this time in a single statement.
            }
            else
            {
                return _dataRepository.GetQuestionsBySearch(search);
                // if we have a search value:
            }


        }
        
        [HttpGet("unanswered")]
        public IEnumerable<QuestionGetManyResponse> GetUnansweredQuestions()
        {
            return _dataRepository.GetUnansweredQuestions();
        }
      
        [HttpGet("{questionId}")]
        public ActionResult<QuestionGetSingleResponse> GetQuestion(int questionId)
        {
            var question = _dataRepository.GetQuestion(questionId);
            if (question == null)
            {
                return NotFound();
            }
            return question;
        }
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<QuestionGetSingleResponse>> PostQuestionAsync(QuestionPostRequest questionPostRequest)
        {
            var savedQuestion = _dataRepository.PostQuestion(new QuestionPostFullRequest
            {
                Title = questionPostRequest.Title,
                Content = questionPostRequest.Content,
               UserId = User.FindFirst(ClaimTypes.NameIdentifier).Value,
             //  UserName = "TestUser",
               UserName = await GetUserName(),
                Created = DateTime.UtcNow
            });
            return CreatedAtAction(nameof(GetQuestion),
            new { questionId = savedQuestion.QuestionId },
            savedQuestion);
        }//da pitam

        [Authorize(Policy = "MustBeQuestionAuthor")]
        [HttpPut("{questionId}")]
        public ActionResult<QuestionGetSingleResponse> PutQuestion(int questionId, QuestionPutRequest questionPutRequest)
        {
            var question = _dataRepository.GetQuestion(questionId);
            if (question == null)
            {
                return NotFound();
            }
            questionPutRequest.Title = string.IsNullOrEmpty(questionPutRequest.Title) ? question.Title : questionPutRequest.Title;
            questionPutRequest.Content = string.IsNullOrEmpty(questionPutRequest.Content) ? question.Content : questionPutRequest.Content;
            // We use ternary expressions to update the request model with data from the existing question if
            //it hasn't been supplied in the request.
            var savedQuestion = _dataRepository.PutQuestion(questionId, questionPutRequest);
            return savedQuestion;
            //call the data repository to update the question and
            //then return the saved question in the response:
        }
        [Authorize(Policy = "MustBeQuestionAuthor")]
        //We have now applied our authorization policy to updating and deleting a question.
        [HttpDelete("{questionId}")]
        public ActionResult DeleteQuestion(int questionId)
        {
            var question = _dataRepository.GetQuestion(questionId);
            if (question==null)
            {
                return NotFound(); 
            }
            _dataRepository.DeleteQuestion(questionId);
            return NoContent();
        }


        [Authorize]
        [HttpPost("answer")]
        public async Task<ActionResult<AnswerGetResponse>> PostAnswer(AnswerPostRequest answerPostRequest)
        {
            var questionExist = _dataRepository.QuestionExists(answerPostRequest.QuestionId.Value);
            if (!questionExist)
            {
                return NotFound();
            }
            var savedAnswer = _dataRepository.PostAnswer(new AnswerPostFullRequest
            { 
                QuestionId=answerPostRequest.QuestionId.Value,
                Content=answerPostRequest.Content,
                UserId = User.FindFirst(ClaimTypes.NameIdentifier).Value,
                UserName = await GetUserName(),
                Created =DateTime.UtcNow
            });
          
            await _questionHubContext.Clients.Group(//get access to the SignalR group through the Group method in the Clients property
                $"Question-{answerPostRequest.QuestionId.Value}")
                    .SendAsync(//push the question with the new answer to all the clients in the group
                        "ReceiveQuestion",
                        _dataRepository.GetQuestion(
                            answerPostRequest.QuestionId.Value));
            // push updated questions to subscribed clients.
            return savedAnswer;
           
        }
       
        private async Task<string> GetUserName()
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                _authOUserInfo
                );
            request.Headers.Add("Authorization", Request.Headers["Authorization"].First());
//            This HTTP header will contain the access
//token that will give us access to the Auth0 endpoint.
//If the
            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request);
            //If the request is successful, we parse the response body into our User model.
            if (response.IsSuccessStatusCode)
            {
                var jsonContent = await response.Content.ReadAsStringAsync();
                var user = JsonSerializer.Deserialize<User>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                    //case-insensitive property mapping so that
//the camel case fields in the response map correctly to the title case
//properties in the class.
                });
                return user.Name;
            }
            else
            {
                return "";
            }
        }
    }
}
