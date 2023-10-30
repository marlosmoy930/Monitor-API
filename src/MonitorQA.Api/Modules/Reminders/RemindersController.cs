using Microsoft.AspNetCore.Mvc;
using MonitorQA.Cloud.Messaging;
using MonitorQA.Notifications.Audits;
using MonitorQA.Notifications.CorrectiveActions;
using System.Threading.Tasks;

namespace MonitorQA.Api.Modules.Reminders
{
    [Route("api/reminders")]
    [ApiController]
    public class RemindersController : ControllerBase
    {
        private readonly CloudMessagePublisher _publisher;

        public RemindersController(
            CloudMessagePublisher publisher)
        {
            this._publisher = publisher;
        }

        [HttpGet]
        [Route("audits/start-tomorrow")]
        public async Task<IActionResult> AuditsStartTomorrow()
        {
            await _publisher.Publish(new AuditGeneralEventMessage() { EventType = AuditGeneralEventType.StartTomorrow });
            return Ok();
        }

        [HttpGet]
        [Route("audits/due-today")]
        public async Task<IActionResult> DueTodayAudits()
        {
            await _publisher.Publish(new AuditGeneralEventMessage() { EventType = AuditGeneralEventType.DueToday });
            return Ok();
        }

        [HttpGet]
        [Route("audits/upcoming")]
        public async Task<IActionResult> UpcomingAudits()
        {
            await _publisher.Publish(new AuditGeneralEventMessage() { EventType = AuditGeneralEventType.Upcomining });
            return Ok();
        }

        [HttpGet]
        [Route("audits/over-due")]
        public async Task<IActionResult> OverDueAudits()
        {
            await _publisher.Publish(new AuditGeneralEventMessage() { EventType = AuditGeneralEventType.OverDue });
            return Ok();
        }

        [HttpGet]
        [Route("audits/in-progress")]
        public async Task<IActionResult> AuditsInProgress()
        {
            await _publisher.Publish(new AuditGeneralEventMessage()
            {
                EventType = AuditGeneralEventType.InProgress
            });
            return Ok();
        }

        [HttpGet]
        [Route("actions/due-today")]
        public async Task<IActionResult> DueTodayActions()
        {
            await _publisher.Publish(new ActionGeneralEventMessage()
            {
                EventType = ActionGeneralEventType.DueToday
            });
            return Ok();
        }

        [HttpGet]
        [Route("actions/over-due")]
        public async Task<IActionResult> OverDueActions()
        {
            await _publisher.Publish(new ActionGeneralEventMessage()
            {
                EventType = ActionGeneralEventType.OverDue
            });
            return Ok();
        }
    }
}
