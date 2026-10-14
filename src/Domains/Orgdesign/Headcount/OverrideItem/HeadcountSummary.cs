namespace Acme.Workforce.Orgdesign.Headcount.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a headcount summary is in its life cycle.</summary>
public enum HeadcountSummaryStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A headcount summary as the headcount team keeps it.</summary>
public sealed class HeadcountSummary(string code, string name, double score, int limit, double threshold, string timezone, HeadcountSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Score { get; set; } = score;
    public int Limit { get; set; } = limit;
    public double Threshold { get; set; } = threshold;
    public string Timezone { get; set; } = timezone;
    public HeadcountSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a headcount summary in status new.</summary>
    public static HeadcountSummary NewHeadcountSummary(string code, string name)
    {
        return new HeadcountSummary(code, name, 0.0, 0, 0.0, "", HeadcountSummaryStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HeadcountSummaryStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HeadcountSummaryStatus next)
    {
        switch (Status)
        {
            case HeadcountSummaryStatus.New:
                switch (next)
                {
                    case HeadcountSummaryStatus.Failed:
                        return true;
                }
                return false;
            case HeadcountSummaryStatus.Queued:
                switch (next)
                {
                    case HeadcountSummaryStatus.Running:
                    case HeadcountSummaryStatus.Done:
                        return true;
                }
                return false;
            case HeadcountSummaryStatus.Running:
                return next == HeadcountSummaryStatus.Done;
            case HeadcountSummaryStatus.Done:
                switch (next)
                {
                    case HeadcountSummaryStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(HeadcountSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"headcount summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double EffectiveExposure()
    {
        return Math.Round(Score + (double)Limit, 2);
    }

    public HeadcountSummary Copy()
    {
        return new HeadcountSummary(Code, Name, Score, Limit, Threshold, Timezone, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
