namespace Acme.Workforce.Absence.Balance.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped balance request is in its life cycle.</summary>
public enum CappedBalanceRequestStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One capped balance request, with what the planners need to act on it.</summary>
public sealed class CappedBalanceRequest(string code, string title, double maximum, int quantity, string channel, string description, List<string> tags, string grade, double fte, string locale, double rate, CappedBalanceRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Maximum { get; set; } = maximum;
    public int Quantity { get; set; } = quantity;
    public string Channel { get; set; } = channel;
    public string Description { get; set; } = description;
    public List<string> Tags { get; set; } = tags;
    public string Grade { get; set; } = grade;
    public double Fte { get; set; } = fte;
    public string Locale { get; set; } = locale;
    public double Rate { get; set; } = rate;
    public CappedBalanceRequestStatus Status { get; set; } = status;

    /// <summary>Starts a capped balance request in status new.</summary>
    public static CappedBalanceRequest NewCappedBalanceRequest(string code, string title)
    {
        return new CappedBalanceRequest(
            code,
            title,
            0.0,
            0,
            "",
            "",
            new List<string>(),
            "",
            0.0,
            "",
            0.0,
            CappedBalanceRequestStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedBalanceRequestStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedBalanceRequestStatus next)
    {
        switch (Status)
        {
            case CappedBalanceRequestStatus.New:
                return next == CappedBalanceRequestStatus.Running;
            case CappedBalanceRequestStatus.Queued:
                return next == CappedBalanceRequestStatus.Running || next == CappedBalanceRequestStatus.Failed;
            case CappedBalanceRequestStatus.Running:
                return next == CappedBalanceRequestStatus.Failed;
            case CappedBalanceRequestStatus.Done:
                return next == CappedBalanceRequestStatus.Failed;
        }
        return false;
    }

    public void MoveTo(CappedBalanceRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped balance request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
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

    public double NetVolume()
    {
        return Math.Round(Rate - Fte, 2);
    }

    public CappedBalanceRequest Copy()
    {
        return new CappedBalanceRequest(
            Code,
            Title,
            Maximum,
            Quantity,
            Channel,
            Description,
            Tags,
            Grade,
            Fte,
            Locale,
            Rate,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
