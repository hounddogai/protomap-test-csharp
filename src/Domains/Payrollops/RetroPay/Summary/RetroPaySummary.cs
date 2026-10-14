namespace Acme.Workforce.Payrollops.RetroPay.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a retro pay summary is in its life cycle.</summary>
public enum RetroPaySummaryStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A retro pay summary. Its code is unique within a tenant.</summary>
public sealed class RetroPaySummary(string code, string label, double maximum, string badge, double budget, string skill, List<string> tags, RetroPaySummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Maximum { get; set; } = maximum;
    public string Badge { get; set; } = badge;
    public double Budget { get; set; } = budget;
    public string Skill { get; set; } = skill;
    public List<string> Tags { get; set; } = tags;
    public RetroPaySummaryStatus Status { get; set; } = status;

    /// <summary>Starts a retro pay summary in status proposed.</summary>
    public static RetroPaySummary NewRetroPaySummary(string code, string label)
    {
        return new RetroPaySummary(
            code,
            label,
            0.0,
            "",
            0.0,
            "",
            new List<string>(),
            RetroPaySummaryStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RetroPaySummaryStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RetroPaySummaryStatus next)
    {
        switch (Status)
        {
            case RetroPaySummaryStatus.Proposed:
                return next == RetroPaySummaryStatus.Approved;
            case RetroPaySummaryStatus.Approved:
                switch (next)
                {
                    case RetroPaySummaryStatus.Rejected:
                        return true;
                }
                return false;
            case RetroPaySummaryStatus.Rejected:
                switch (next)
                {
                    case RetroPaySummaryStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RetroPaySummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"retro pay summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double EffectiveValue()
    {
        return Math.Round(Maximum - Budget, 2);
    }

    public RetroPaySummary Copy()
    {
        return new RetroPaySummary(Code, Label, Maximum, Badge, Budget, Skill, Tags, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
