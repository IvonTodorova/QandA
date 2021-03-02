using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
namespace QandA.Authorization
{
    //Our MustBeQuestionAuthorHandler class will need access to the HTTP requests to find out the question that is
   // being requested.
    using Microsoft.AspNetCore.Authorization;
    namespace QandA.Authorization
    {
        public class MustBeQuestionAuthorRequirement :
        IAuthorizationRequirement
        {
            public MustBeQuestionAuthorRequirement()
            {

            }
        }
    }
}
