namespace Acme.Workforce.Talent.SkillProfile.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive skill profile summary is in its life cycle.</summary>
public enum AdaptiveSkillProfileSummaryStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A adaptive skill profile summary as the skill profile team keeps it.</summary>
public sealed class AdaptiveSkillProfileSummary(string code, string name, bool archived, int windowDays, string summary, AdaptiveSkillProfileSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Archived { get; set; } = archived;
    public int WindowDays { get; set; } = windowDays;
    public string Summary { get; set; } = summary;
    public AdaptiveSkillProfileSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive skill profile summary in status new.</summary>
    public static AdaptiveSkillProfileSummary NewAdaptiveSkillProfileSummary(string code, string name)
    {
        return new AdaptiveSkillProfileSummary(code, name, false, 0, "", AdaptiveSkillProfileSummaryStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveSkillProfileSummaryStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveSkillProfileSummaryStatus next)
    {
        switch (Status)
        {
            case AdaptiveSkillProfileSummaryStatus.New:
                switch (next)
                {
                    case AdaptiveSkillProfileSummaryStatus.Queued:
                        return true;
                }
                return false;
            case AdaptiveSkillProfileSummaryStatus.Queued:
                switch (next)
                {
                    case AdaptiveSkillProfileSummaryStatus.Failed:
                        return true;
                }
                return false;
            case AdaptiveSkillProfileSummaryStatus.Running:
                switch (next)
                {
                    case AdaptiveSkillProfileSummaryStatus.Failed:
                        return true;
                }
                return false;
            case AdaptiveSkillProfileSummaryStatus.Done:
                return next == AdaptiveSkillProfileSummaryStatus.Failed;
        }
        return false;
    }

    public void MoveTo(AdaptiveSkillProfileSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive skill profile summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"adaptive skill profile summary"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)WindowDays;
    }

    public AdaptiveSkillProfileSummary Copy()
    {
        return new AdaptiveSkillProfileSummary(Code, Name, Archived, WindowDays, Summary, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
