using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GiaoHangTietKiem.Models
{
    public class ForgetPass
    {
        public string Pass { set; get; }
        public string AgainPass { set; get; }
    }
}