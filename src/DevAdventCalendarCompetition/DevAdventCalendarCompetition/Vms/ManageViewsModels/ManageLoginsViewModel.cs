﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DevAdventCalendarCompetition.Vms
{
    public class ManageLoginsViewModel
    {
        public IList<UserLoginInfo> CurrentLogins { get; private set; }

        public IList<AuthenticationDescription> OtherLogins { get; private set; }

        public IList<AuthenticationScheme> ExternalProviders { get; private set; }
    }
}