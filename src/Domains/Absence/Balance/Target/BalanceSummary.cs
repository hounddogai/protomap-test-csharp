namespace Acme.Workforce.Absence.Balance.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a balance summary is in its life cycle.</summary>
public enum BalanceSummaryStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A balance summary as the balance team keeps it.</summary>
public sealed class BalanceSummary(string code, string name, string checksum, string label, int limit, double maximum, BalanceSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Checksum { get; set; } = checksum;
    public string Label { get; set; } = label;
    public int Limit { get; set; } = limit;
    public double Maximum { get; set; } = maximum;
    public BalanceSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a balance summary in status requested.</summary>
    public static BalanceSummary NewBalanceSummary(string code, string name)
    {
        return new BalanceSummary(code, name, "", "", 0, 0.0, BalanceSummaryStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BalanceSummaryStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BalanceSummaryStatus next)
    {
        switch (Status)
        {
            case BalanceSummaryStatus.Requested:
                switch (next)
                {
                    case BalanceSummaryStatus.Expired:
                        return true;
                }
                return false;
            case BalanceSummaryStatus.Granted:
                return next == BalanceSummaryStatus.Expired;
            case BalanceSummaryStatus.Revoked:
                return next == BalanceSummaryStatus.Expired;
        }
        return false;
    }

    public void MoveTo(BalanceSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"balance summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Name}";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)Limit * Maximum, 2);
    }

    public BalanceSummary Copy()
    {
        return new BalanceSummary(Code, Name, Checksum, Label, Limit, Maximum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
