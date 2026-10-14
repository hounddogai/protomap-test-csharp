namespace Acme.Workforce.Compensation.BonusPool.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual bonus pool draft is in its life cycle.</summary>
public enum ManualBonusPoolDraftStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A manual bonus pool draft as the bonus pool team keeps it.</summary>
public sealed class ManualBonusPoolDraft(string code, string name, string channel, double budget, string summary, double hours, ManualBonusPoolDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Channel { get; set; } = channel;
    public double Budget { get; set; } = budget;
    public string Summary { get; set; } = summary;
    public double Hours { get; set; } = hours;
    public ManualBonusPoolDraftStatus Status { get; set; } = status;

    /// <summary>Starts a manual bonus pool draft in status requested.</summary>
    public static ManualBonusPoolDraft NewManualBonusPoolDraft(string code, string name)
    {
        return new ManualBonusPoolDraft(code, name, "", 0.0, "", 0.0, ManualBonusPoolDraftStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualBonusPoolDraftStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualBonusPoolDraftStatus next)
    {
        switch (Status)
        {
            case ManualBonusPoolDraftStatus.Requested:
                return next == ManualBonusPoolDraftStatus.Granted || next == ManualBonusPoolDraftStatus.Expired;
            case ManualBonusPoolDraftStatus.Granted:
                switch (next)
                {
                    case ManualBonusPoolDraftStatus.Revoked:
                        return true;
                }
                return false;
            case ManualBonusPoolDraftStatus.Revoked:
                switch (next)
                {
                    case ManualBonusPoolDraftStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ManualBonusPoolDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual bonus pool draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double WeightedLoad()
    {
        return Math.Round(Hours - Budget, 2);
    }

    public ManualBonusPoolDraft Copy()
    {
        return new ManualBonusPoolDraft(Code, Name, Channel, Budget, Summary, Hours, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
