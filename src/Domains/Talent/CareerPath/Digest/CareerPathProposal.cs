namespace Acme.Workforce.Talent.CareerPath.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a career path proposal is in its life cycle.</summary>
public enum CareerPathProposalStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A career path proposal. Its code is unique within a tenant.</summary>
public sealed class CareerPathProposal(string code, string name, string phone, double fte, double ratio, int sequence, double maximum, string currency, int graceDays, double amount, CareerPathProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Phone { get; set; } = phone;
    public double Fte { get; set; } = fte;
    public double Ratio { get; set; } = ratio;
    public int Sequence { get; set; } = sequence;
    public double Maximum { get; set; } = maximum;
    public string Currency { get; set; } = currency;
    public int GraceDays { get; set; } = graceDays;
    public double Amount { get; set; } = amount;
    public CareerPathProposalStatus Status { get; set; } = status;

    /// <summary>Starts a career path proposal in status idle.</summary>
    public static CareerPathProposal NewCareerPathProposal(string code, string name)
    {
        return new CareerPathProposal(code, name, "", 0.0, 0.0, 0, 0.0, "", 0, 0.0, CareerPathProposalStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CareerPathProposalStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CareerPathProposalStatus next)
    {
        switch (Status)
        {
            case CareerPathProposalStatus.Idle:
                switch (next)
                {
                    case CareerPathProposalStatus.Ready:
                        return true;
                }
                return false;
            case CareerPathProposalStatus.Warming:
                return next == CareerPathProposalStatus.Draining || next == CareerPathProposalStatus.Stopped;
            case CareerPathProposalStatus.Ready:
                switch (next)
                {
                    case CareerPathProposalStatus.Draining:
                        return true;
                }
                return false;
            case CareerPathProposalStatus.Draining:
                return next == CareerPathProposalStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(CareerPathProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"career path proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double NetTotal()
    {
        return Math.Round(Fte + Maximum, 2);
    }

    public CareerPathProposal Copy()
    {
        return new CareerPathProposal(
            Code,
            Name,
            Phone,
            Fte,
            Ratio,
            Sequence,
            Maximum,
            Currency,
            GraceDays,
            Amount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
