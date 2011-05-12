﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NzbDrone.Web.Models
{
    public class MissingEpisodeModel
    {
        public int EpisodeId { get; set; }
        public string SeriesTitle { get; set; }
        public int SeasonNumber { get; set; }
        public int EpisodeNumber { get; set; }
        public string EpisodeTitle { get; set; }
        public DateTime AirDate { get; set; }
        public string Overview { get; set; }
    }
}