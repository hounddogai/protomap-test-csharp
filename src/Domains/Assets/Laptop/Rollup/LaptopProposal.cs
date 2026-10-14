namespace Acme.Workforce.Assets.Laptop.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a laptop proposal is in its life cycle.</summary>
public enum LaptopProposalStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A laptop proposal. Its code is unique within a tenant.</summary>
public sealed class LaptopProposal(string code, string title, int retries, int version, bool active, string jobCode, double amount, int attempts, string externalRef, string channel, int graceDays, LaptopProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Retries { get; set; } = retries;
    public int Version { get; set; } = version;
    public bool Active { get; set; } = active;
    public string JobCode { get; set; } = jobCode;
    public double Amount { get; set; } = amount;
    public int Attempts { get; set; } = attempts;
    public string ExternalRef { get; set; } = externalRef;
    public string Channel { get; set; } = channel;
    public int GraceDays { get; set; } = graceDays;
    public LaptopProposalStatus Status { get; set; } = status;

    /// <summary>Starts a laptop proposal in status created.</summary>
    public static LaptopProposal NewLaptopProposal(string code, string title)
    {
        return new LaptopProposal(code, title, 0, 0, false, "", 0.0, 0, "", "", 0, LaptopProposalStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LaptopProposalStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LaptopProposalStatus next)
    {
        switch (Status)
        {
            case LaptopProposalStatus.Created:
                return next == LaptopProposalStatus.Retired;
            case LaptopProposalStatus.Validated:
                return next == LaptopProposalStatus.Published || next == LaptopProposalStatus.Retired;
            case LaptopProposalStatus.Published:
                return next == LaptopProposalStatus.Retired;
        }
        return false;
    }

    public void MoveTo(LaptopProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"laptop proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)Attempts + (double)Retries, 2);
    }

    public LaptopProposal Copy()
    {
        return new LaptopProposal(
            Code,
            Title,
            Retries,
            Version,
            Active,
            JobCode,
            Amount,
            Attempts,
            ExternalRef,
            Channel,
            GraceDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
