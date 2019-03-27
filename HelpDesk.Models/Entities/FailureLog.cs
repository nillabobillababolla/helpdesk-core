﻿using HelpDesk.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace HelpDesk.Models.Entities
{
    public class FailureLog : BaseEntity<int>
    {
        public string Message { get; set; }
        public IdentityRoles FromWhom { get; set; }

        public int FailureId { get; set; }

        [ForeignKey("FailureId")]
        public virtual Failure Failure { get; set; }
    }
}
