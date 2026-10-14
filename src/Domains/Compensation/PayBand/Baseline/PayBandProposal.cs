namespace Acme.Workforce.Compensation.PayBand.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay band proposal is in its life cycle.</summary>
public enum PayBandProposalStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A pay band proposal. Its code is unique within a tenant.</summary>
public sealed class PayBandProposal(string code, string title, bool approved, string name, int windowDays, string phone, PayBandProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Approved { get; set; } = approved;
    public string Name { get; set; } = name;
    public int WindowDays { get; set; } = windowDays;
    public string Phone { get; set; } = phone;
    public PayBandProposalStatus Status { get; set; } = status;

    /// <summary>Starts a pay band proposal in status draft.</summary>
    public static PayBandProposal NewPayBandProposal(string code, string title)
    {
        return new PayBandProposal(code, title, false, "", 0, "", PayBandProposalStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayBandProposalStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayBandProposalStatus next)
    {
        switch (Status)
        {
            case PayBandProposalStatus.Draft:
                switch (next)
                {
                    case PayBandProposalStatus.Pending:
                    case PayBandProposalStatus.Active:
                        return true;
                }
                return false;
            case PayBandProposalStatus.Pending:
                return next == PayBandProposalStatus.Closed;
            case PayBandProposalStatus.Active:
                switch (next)
                {
                    case PayBandProposalStatus.Closed:
                        return true;
                }
                return false;
            case PayBandProposalStatus.Suspended:
                switch (next)
                {
                    case PayBandProposalStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PayBandProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay band proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double MeasuredValue()
    {
        return (double)WindowDays;
    }

    public PayBandProposal Copy()
    {
        return new PayBandProposal(Code, Title, Approved, Name, WindowDays, Phone, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
