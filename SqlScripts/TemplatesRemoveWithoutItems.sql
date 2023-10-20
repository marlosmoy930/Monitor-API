delete tia
from TemplateItemAnswer tia
where tia.TemplateItemId in  
(
	select ti.Id
	from TemplateItems ti
	left join TemplateItemAnswer tia2 on tia2.TemplateItemId = ti.Id
	where tia2.Id is null
)

delete templateItem
from TemplateItems templateItem
where templateItem.TemplateId in 
(
	select t.Id
	from Templates t
	left join TemplateItems ti on ti.TemplateId = t.Id
	where ti.Id is null
)

delete tag
from TemplateTag tag
where tag.TemplateId in 
(
	select t.Id
	from Templates t
	left join TemplateItems ti on ti.TemplateId = t.Id
	where ti.Id is null
)

delete assignee
from AuditAssignee assignee
where assignee.AuditId in 
(
	select a.Id
	from Audits a
	inner join AuditSchedules s on s.Id = a.AuditScheduleId
	inner join Templates t on s.TemplateId = t.Id
	left join TemplateItems ti on ti.TemplateId = t.Id
	where ti.Id is null
)

delete n
from Notifications n
where n.AuditId in 
(
	select a.Id
	from Audits a
	inner join AuditSchedules s on s.Id = a.AuditScheduleId
	inner join Templates t on s.TemplateId = t.Id
	left join TemplateItems ti on ti.TemplateId = t.Id
	where ti.Id is null
)

delete aa
from Audits aa
where aa.AuditScheduleId in 
(
	select s.Id
	from AuditSchedules s
	inner join Templates t on s.TemplateId = t.Id
	left join TemplateItems ti on ti.TemplateId = t.Id
	where ti.Id is null
)

delete tag
from AuditScheduleTag tag
where tag.AuditScheduleId in 
(
	select s.Id
	from AuditSchedules s
	inner join Templates t on s.TemplateId = t.Id
	left join TemplateItems ti on ti.TemplateId = t.Id
	where ti.Id is null
)

delete schedule
from AuditSchedules schedule
where schedule.TemplateId in 
(
	select t.Id
	from Templates t
	left join TemplateItems ti on ti.TemplateId = t.Id
	where ti.Id is null
)

delete t
from Templates t
left join TemplateItems ti on ti.TemplateId = t.Id
where ti.Id is null

-- template without items
select t.Id, ti.Id, ti.Text
from Templates t
left join TemplateItems ti on ti.TemplateId = t.Id
where ti.Id is null
