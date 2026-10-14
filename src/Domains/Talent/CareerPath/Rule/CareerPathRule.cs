namespace Acme.Workforce.Talent.CareerPath.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a career path rule is in its life cycle.</summary>
public enum CareerPathRuleStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A career path rule. Its code is unique within a tenant.</summary>
public sealed class CareerPathRule(string code, string title, int sequence, string timezone, double threshold, string grade, CareerPathRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Sequence { get; set; } = sequence;
    public string Timezone { get; set; } = timezone;
    public double Threshold { get; set; } = threshold;
    public string Grade { get; set; } = grade;
    public CareerPathRuleStatus Status { get; set; } = status;

    /// <summary>Starts a career path rule in status created.</summary>
    public static CareerPathRule NewCareerPathRule(string code, string title)
    {
        return new CareerPathRule(code, title, 0, "", 0.0, "", CareerPathRuleStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CareerPathRuleStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CareerPathRuleStatus next)
    {
        switch (Status)
        {
            case CareerPathRuleStatus.Created:
                return next == CareerPathRuleStatus.Published || next == CareerPathRuleStatus.Retired;
            case CareerPathRuleStatus.Validated:
                switch (next)
                {
                    case CareerPathRuleStatus.Published:
                    case CareerPathRuleStatus.Retired:
                        return true;
                }
                return false;
            case CareerPathRuleStatus.Published:
                switch (next)
                {
                    case CareerPathRuleStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CareerPathRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"career path rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)Sequence - Threshold, 2);
    }

    public CareerPathRule Copy()
    {
        return new CareerPathRule(Code, Title, Sequence, Timezone, Threshold, Grade, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
