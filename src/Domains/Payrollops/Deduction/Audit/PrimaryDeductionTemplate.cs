namespace Acme.Workforce.Payrollops.Deduction.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary deduction template is in its life cycle.</summary>
public enum PrimaryDeductionTemplateStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A primary deduction template. Its code is unique within a tenant.</summary>
public sealed class PrimaryDeductionTemplate(string code, string title, double score, string phone, int leaveDays, double weight, string currency, int capacity, bool archived, string unit, int graceDays, PrimaryDeductionTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Score { get; set; } = score;
    public string Phone { get; set; } = phone;
    public int LeaveDays { get; set; } = leaveDays;
    public double Weight { get; set; } = weight;
    public string Currency { get; set; } = currency;
    public int Capacity { get; set; } = capacity;
    public bool Archived { get; set; } = archived;
    public string Unit { get; set; } = unit;
    public int GraceDays { get; set; } = graceDays;
    public PrimaryDeductionTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a primary deduction template in status draft.</summary>
    public static PrimaryDeductionTemplate NewPrimaryDeductionTemplate(string code, string title)
    {
        return new PrimaryDeductionTemplate(
            code,
            title,
            0.0,
            "",
            0,
            0.0,
            "",
            0,
            false,
            "",
            0,
            PrimaryDeductionTemplateStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryDeductionTemplateStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryDeductionTemplateStatus next)
    {
        switch (Status)
        {
            case PrimaryDeductionTemplateStatus.Draft:
                return next == PrimaryDeductionTemplateStatus.Closed;
            case PrimaryDeductionTemplateStatus.Pending:
                return next == PrimaryDeductionTemplateStatus.Active;
            case PrimaryDeductionTemplateStatus.Active:
                switch (next)
                {
                    case PrimaryDeductionTemplateStatus.Suspended:
                    case PrimaryDeductionTemplateStatus.Closed:
                        return true;
                }
                return false;
            case PrimaryDeductionTemplateStatus.Suspended:
                switch (next)
                {
                    case PrimaryDeductionTemplateStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryDeductionTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary deduction template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)LeaveDays + Score, 2);
    }

    public PrimaryDeductionTemplate Copy()
    {
        return new PrimaryDeductionTemplate(
            Code,
            Title,
            Score,
            Phone,
            LeaveDays,
            Weight,
            Currency,
            Capacity,
            Archived,
            Unit,
            GraceDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
