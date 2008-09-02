using System.Web.Mvc;
using CodeCampServer.Model;
using CodeCampServer.Model.Domain;
using MvcContrib;

namespace CodeCampServer.Website.Controllers
{    
    public class AttendeeController : BaseController
    {
        private readonly IConferenceRepository _conferenceRepository;

        public AttendeeController(IUserSession userSession, IConferenceRepository conferenceRepository) : 
            base(userSession)
        {
            _conferenceRepository = conferenceRepository;
        }

        public ActionResult List(string conferenceKey)
        {
            var conf = _conferenceRepository.GetConferenceByKey(conferenceKey);            
            var attendees = conf.GetAttendees();            

            ViewData.Add(attendees);
            return View();
        }
    }
}