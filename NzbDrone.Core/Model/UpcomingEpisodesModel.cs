﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Model
{
    public class UpcomingEpisodesModel
    {
        public List<Episode> Yesterday { get; set; }
        public List<Episode> Today { get; set; }
        public List<Episode> Week { get; set; }
    }
}