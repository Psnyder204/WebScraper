using System;

namespace Models.Requests.DiplomaticMissions
{
    public class DiplomaticMissionAddRequest
    {

        public int CountryId { get; set; }
        public string Name { get; set; }
        public int LocationId { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public bool IsEmbassy { get; set; }
        public bool IsConsulate { get; set; }

    }
}
