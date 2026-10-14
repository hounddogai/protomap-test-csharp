namespace Acme.Workforce.Orgdesign.CostModel.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback cost model score is in its life cycle.</summary>
public enum FallbackCostModelScoreStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A fallback cost model score. Its code is unique within a tenant.</summary>
public sealed class FallbackCostModelScore(string code, string title, double margin, string grade, double rating, int leaveDays, FallbackCostModelScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Margin { get; set; } = margin;
    public string Grade { get; set; } = grade;
    public double Rating { get; set; } = rating;
    public int LeaveDays { get; set; } = leaveDays;
    public FallbackCostModelScoreStatus Status { get; set; } = status;

    /// <summary>Starts a fallback cost model score in status created.</summary>
    public static FallbackCostModelScore NewFallbackCostModelScore(string code, string title)
    {
        return new FallbackCostModelScore(code, title, 0.0, "", 0.0, 0, FallbackCostModelScoreStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackCostModelScoreStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackCostModelScoreStatus next)
    {
        switch (Status)
        {
            case FallbackCostModelScoreStatus.Created:
                return next == FallbackCostModelScoreStatus.Published || next == FallbackCostModelScoreStatus.Retired;
            case FallbackCostModelScoreStatus.Validated:
                switch (next)
                {
                    case FallbackCostModelScoreStatus.Published:
                        return true;
                }
                return false;
            case FallbackCostModelScoreStatus.Published:
                switch (next)
                {
                    case FallbackCostModelScoreStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackCostModelScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback cost model score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double AdjustedLoad()
    {
        return Math.Round(Margin + (double)LeaveDays, 2);
    }

    public FallbackCostModelScore Copy()
    {
        return new FallbackCostModelScore(Code, Title, Margin, Grade, Rating, LeaveDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
