﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hidesk.Shared.Messages
{
    [Serializable]
    public class ValidationRequest : RequestMessageBase
    {
        public String Email { get; set; }
    }
}
