namespace Acme.Workforce.Payrollops.RetroPay.PayAssignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict retro pay proposal is in its life cycle.</summary>
public enum StrictRetroPayProposalStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One strict retro pay proposal, with what the planners need to act on it.</summary>
public sealed class StrictRetroPayProposal(string code, string name, double weight, string label, double floorValue, string category, double discount, StrictRetroPayProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Weight { get; set; } = weight;
    public string Label { get; set; } = label;
    public double FloorValue { get; set; } = floorValue;
    public string Category { get; set; } = category;
    public double Discount { get; set; } = discount;
    public StrictRetroPayProposalStatus Status { get; set; } = status;

    /// <summary>Starts a strict retro pay proposal in status draft.</summary>
    public static StrictRetroPayProposal NewStrictRetroPayProposal(string code, string name)
    {
        return new StrictRetroPayProposal(code, name, 0.0, "", 0.0, "", 0.0, StrictRetroPayProposalStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictRetroPayProposalStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictRetroPayProposalStatus next)
    {
        switch (Status)
        {
            case StrictRetroPayProposalStatus.Draft:
                switch (next)
                {
                    case StrictRetroPayProposalStatus.Pending:
                    case StrictRetroPayProposalStatus.Suspended:
                        return true;
                }
                return false;
            case StrictRetroPayProposalStatus.Pending:
                return next == StrictRetroPayProposalStatus.Suspended;
            case StrictRetroPayProposalStatus.Active:
                switch (next)
                {
                    case StrictRetroPayProposalStatus.Suspended:
                    case StrictRetroPayProposalStatus.Closed:
                        return true;
                }
                return false;
            case StrictRetroPayProposalStatus.Suspended:
                return next == StrictRetroPayProposalStatus.Closed;
        }
        return false;
    }

    public void MoveTo(StrictRetroPayProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict retro pay proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"strict retro pay proposal"} {Code}";
    }

    public double EffectiveExposure()
    {
        return Math.Round(Weight * Discount, 2);
    }

    public StrictRetroPayProposal Copy()
    {
        return new StrictRetroPayProposal(Code, Name, Weight, Label, FloorValue, Category, Discount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
