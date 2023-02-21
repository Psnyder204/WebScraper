using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sabio.Models.Requests.DiplomaticMissions
{
    public class DiplomaticMissionUpdateRequest : DiplomaticMissionAddRequest, IModelIdentifier
    {

        public int Id { get; set; }
        public bool IsActive { get; set; }
    }
}
