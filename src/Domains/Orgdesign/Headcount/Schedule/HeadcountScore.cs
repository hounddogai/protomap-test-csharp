namespace Acme.Workforce.Orgdesign.Headcount.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a headcount score is in its life cycle.</summary>
public enum HeadcountScoreStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One headcount score, with what the planners need to act on it.</summary>
public sealed class HeadcountScore(string code, string title, string reviewer, int durationMinutes, string source, string region, double margin, string grade, int retries, double threshold, string jobCode, HeadcountScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Reviewer { get; set; } = reviewer;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Source { get; set; } = source;
    public string Region { get; set; } = region;
    public double Margin { get; set; } = margin;
    public string Grade { get; set; } = grade;
    public int Retries { get; set; } = retries;
    public double Threshold { get; set; } = threshold;
    public string JobCode { get; set; } = jobCode;
    public HeadcountScoreStatus Status { get; set; } = status;

    /// <summary>Starts a headcount score in status requested.</summary>
    public static HeadcountScore NewHeadcountScore(string code, string title)
    {
        return new HeadcountScore(code, title, "", 0, "", "", 0.0, "", 0, 0.0, "", HeadcountScoreStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HeadcountScoreStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HeadcountScoreStatus next)
    {
        switch (Status)
        {
            case HeadcountScoreStatus.Requested:
                switch (next)
                {
                    case HeadcountScoreStatus.Revoked:
                        return true;
                }
                return false;
            case HeadcountScoreStatus.Granted:
                switch (next)
                {
                    case HeadcountScoreStatus.Revoked:
                    case HeadcountScoreStatus.Expired:
                        return true;
                }
                return false;
            case HeadcountScoreStatus.Revoked:
                return next == HeadcountScoreStatus.Expired;
        }
        return false;
    }

    public void MoveTo(HeadcountScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"headcount score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"headcount score"} {Code}";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)Retries * (double)DurationMinutes, 2);
    }

    public HeadcountScore Copy()
    {
        return new HeadcountScore(
            Code,
            Title,
            Reviewer,
            DurationMinutes,
            Source,
            Region,
            Margin,
            Grade,
            Retries,
            Threshold,
            JobCode,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
