using System;
using System.Collections.Generic;
using System.Text;

namespace BMS.Infrastructure.Authentication.Models
{
    public class TokenResponse
    {
        public string id { get; set; }
        public string auth_token { get; set; }
        public string refresh_token { get; set; }
        public int expires_in { get; set; }
    }
}
