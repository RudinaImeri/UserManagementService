using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManagement.Domain.Common.Response
{
    public class ResponseWrapper<T> where T : class
    {
        public ResponseWrapper() { }

        public ResponseWrapper(T value)
        {
            Data = value;
        }

        public T Data { get; set; }
        public int TotalPages { get; set; }
        public string Message { get; set; } = "Success";
        public int StatusCode { get; set; } = StatusCodes.Status200OK;
    }
}
