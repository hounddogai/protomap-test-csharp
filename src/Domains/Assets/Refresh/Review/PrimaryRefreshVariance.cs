namespace Acme.Workforce.Assets.Refresh.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary refresh variance is in its life cycle.</summary>
public enum PrimaryRefreshVarianceStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One primary refresh variance, with what the planners need to act on it.</summary>
public sealed class PrimaryRefreshVariance(string code, string label, string phone, int graceDays, string grade, string title, int startsAt, double cost, int revision, double budget, double weight, PrimaryRefreshVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Phone { get; set; } = phone;
    public int GraceDays { get; set; } = graceDays;
    public string Grade { get; set; } = grade;
    public string Title { get; set; } = title;
    public int StartsAt { get; set; } = startsAt;
    public double Cost { get; set; } = cost;
    public int Revision { get; set; } = revision;
    public double Budget { get; set; } = budget;
    public double Weight { get; set; } = weight;
    public PrimaryRefreshVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a primary refresh variance in status draft.</summary>
    public static PrimaryRefreshVariance NewPrimaryRefreshVariance(string code, string label)
    {
        return new PrimaryRefreshVariance(
            code,
            label,
            "",
            0,
            "",
            "",
            0,
            0.0,
            0,
            0.0,
            0.0,
            PrimaryRefreshVarianceStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryRefreshVarianceStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryRefreshVarianceStatus next)
    {
        switch (Status)
        {
            case PrimaryRefreshVarianceStatus.Draft:
                switch (next)
                {
                    case PrimaryRefreshVarianceStatus.Closed:
                        return true;
                }
                return false;
            case PrimaryRefreshVarianceStatus.Pending:
                switch (next)
                {
                    case PrimaryRefreshVarianceStatus.Active:
                    case PrimaryRefreshVarianceStatus.Closed:
                        return true;
                }
                return false;
            case PrimaryRefreshVarianceStatus.Active:
                return next == PrimaryRefreshVarianceStatus.Closed;
            case PrimaryRefreshVarianceStatus.Suspended:
                return next == PrimaryRefreshVarianceStatus.Closed;
        }
        return false;
    }

    public void MoveTo(PrimaryRefreshVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary refresh variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{"primary refresh variance"} {Code}";
    }

    public double WeightedValue()
    {
        return Math.Round((double)Revision * Cost, 2);
    }

    public PrimaryRefreshVariance Copy()
    {
        return new PrimaryRefreshVariance(
            Code,
            Label,
            Phone,
            GraceDays,
            Grade,
            Title,
            StartsAt,
            Cost,
            Revision,
            Budget,
            Weight,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
