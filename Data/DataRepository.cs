using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Dapper;
using QandA.Data.Models;
namespace QandA.Data
{
    public class DataRepository : IDataRepository
    {
        private readonly string _connectionString;
        public DataRepository(IConfiguration configuration)
        {
            _connectionString =
            configuration["ConnectionStrings:DefaultConnection"];
        }

        public AnswerGetResponse GetAnswer(int answerId)
        {
            throw new NotImplementedException();
        }

        public QuestionGetSingleResponse GetQuestion(int questionId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var question =
                connection.QueryFirstOrDefault<QuestionGetSingleResponse>(
                @"EXEC dbo.Question_GetSingle @QuestionId = @QuestionId",
                new { QuestionId = questionId }
                );
                if (question != null)
                {
                    question.Answers =
                connection.Query<AnswerGetResponse>(
                @"EXEC dbo.Answer_Get_ByQuestionId @QuestionId = @QuestionId",
                new { QuestionId = questionId }
                );

                }
                return question;

            }
        }

        public IEnumerable<QuestionGetManyResponse> GetQuestions()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                return connection.Query<QuestionGetManyResponse>(
                    @"EXEC dbo.Question_GetMany"
                );
            }

        }

        public IEnumerable<QuestionGetManyResponse> GetQuestionsBySearch(string search)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                return connection.Query<QuestionGetManyResponse>(
                    @"EXEC dbo.Question_GetMany_BySearch @Search=@Search",
                    new { Search = search }
                    );
            }
        }

        public IEnumerable<QuestionGetManyResponse> GetUnansweredQuestions()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                return connection.Query<QuestionGetManyResponse>(
                    "EXEC dbo.Question_GetUnanswered"
                    );
            }
        }

        public bool QuestionExists(int questionId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                return connection.QueryFirst<bool>(
                    @"EXEC dbo.Question_Exists @QuestionId= @QuestionId",
                    new { QuestionId = questionId }
                    );
            }
        }
        //add new question
        public QuestionGetSingleResponse PostQuestion(QuestionPostFullRequest question)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var questionId = connection.QueryFirst<int>(//da pitam
                @"EXEC dbo.Question_Post
                @Title = @Title, @Content = @Content,
                @UserId = @UserId, @UserName = @UserName,
                @Created = @Created",
                question
                );
                return GetQuestion(questionId);//get the id from the stored procedure for posting a new question 
                //da pitam
            }
        }
        //change question
        public QuestionGetSingleResponse PutQuestion(int questionId, QuestionPutRequest question)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                connection.Execute(
                @"EXEC dbo.Question_Put
                @QuestionId = @QuestionId, @Title = @Title, @Content = @Content",
                new { QuestionId = questionId, Title = question.Title, question.Content }
                );
                return GetQuestion(questionId);//shte promenim questiona v bazata s novoto sadyrjanie update question
            }
        }
        public void DeleteQuestion(int questionId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                connection.Execute(// a tuk dapper naistina ne vryshta nishto 
                @"EXEC dbo.Question_Delete
                @QuestionId = @QuestionId",
                new { QuestionId = questionId }
                );
            }
        }
        //adding an answer to a question
        //the stored procedure returns the saved answer
        public AnswerGetResponse PostAnswer(AnswerPostFullRequest answer)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                return connection.QueryFirst<AnswerGetResponse>(
                @"EXEC dbo.Answer_Post
                @QuestionId = @QuestionId, @Content = @Content,
                @UserId = @UserId, @UserName = @UserName,
                @Created = @Created",
                answer
                );
            }
        }
        public IEnumerable<QuestionGetManyResponse> GetQuestionsWithAnswers()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var questionDictionary =
                new Dictionary<int, QuestionGetManyResponse>();
                return connection
                .Query<
                QuestionGetManyResponse,
                AnswerGetResponse,
                QuestionGetManyResponse>(
                "EXEC dbo.Question_GetMany_WithAnswers",
                map: (q, a) =>
                {
                    QuestionGetManyResponse question;
                    if (!questionDictionary.TryGetValue(q.QuestionId, out question))
                    {
                        question = q;
                        question.Answers =
                             new List<AnswerGetResponse>();
                        questionDictionary.Add(question.QuestionId, question);
                    }
                    question.Answers.Add(a);
                    return question;
                },
                splitOn: "QuestionId"
                )
                .Distinct()
                .ToList();
            }
        }

    }
}
