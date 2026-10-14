namespace Acme.Workforce.Orgdesign.Span.SpanReview;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual span plan is in its life cycle.</summary>
public enum ManualSpanPlanStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A manual span plan. Its code is unique within a tenant.</summary>
public sealed class ManualSpanPlan(string code, string name, double hours, string salaryBand, bool archived, string managerId, int leaveDays, int updatedAt, string skill, int durationMinutes, string phone, ManualSpanPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Hours { get; set; } = hours;
    public string SalaryBand { get; set; } = salaryBand;
    public bool Archived { get; set; } = archived;
    public string ManagerId { get; set; } = managerId;
    public int LeaveDays { get; set; } = leaveDays;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Skill { get; set; } = skill;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Phone { get; set; } = phone;
    public ManualSpanPlanStatus Status { get; set; } = status;

    /// <summary>Starts a manual span plan in status created.</summary>
    public static ManualSpanPlan NewManualSpanPlan(string code, string name)
    {
        return new ManualSpanPlan(code, name, 0.0, "", false, "", 0, 0, "", 0, "", ManualSpanPlanStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualSpanPlanStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualSpanPlanStatus next)
    {
        switch (Status)
        {
            case ManualSpanPlanStatus.Created:
                switch (next)
                {
                    case ManualSpanPlanStatus.Published:
                        return true;
                }
                return false;
            case ManualSpanPlanStatus.Validated:
                switch (next)
                {
                    case ManualSpanPlanStatus.Retired:
                        return true;
                }
                return false;
            case ManualSpanPlanStatus.Published:
                switch (next)
                {
                    case ManualSpanPlanStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ManualSpanPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual span plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double NetVolume()
    {
        return Math.Round(Hours + (double)UpdatedAt, 2);
    }

    public ManualSpanPlan Copy()
    {
        return new ManualSpanPlan(
            Code,
            Name,
            Hours,
            SalaryBand,
            Archived,
            ManagerId,
            LeaveDays,
            UpdatedAt,
            Skill,
            DurationMinutes,
            Phone,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
