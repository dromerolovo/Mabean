using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Mabean.Abstract
{
    public interface IAiService
    {
        Task<string> SendMessageAsync(string userMessage);
    }
}
