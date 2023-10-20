select * 
from Audits

select [Color], count(1) from 
AuditItemAnswers
group by [Color]

delete from AuditItemAnswers where [Name] is null

delete actions 
from CorrectiveActions actions
where actions.AuditItemId in (
	select ai.Id
	from AuditItems ai
	left join AuditItemAnswers aia on aia.AuditItemId = ai.Id
	where aia.Id is null
)

delete auditItem
from AuditItems auditItem
where auditItem.Id in (
	select ai.Id
	from AuditItems ai
	left join AuditItemAnswers aia on aia.AuditItemId = ai.Id
	where aia.Id is null
)

