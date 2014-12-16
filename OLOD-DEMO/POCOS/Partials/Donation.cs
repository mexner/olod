using System;

namespace Pocos
{
    public partial class Donation
    {

        /// <summary>
        /// if donation is refunded, will provide amount of refund (partial & full)
        /// </summary>
        public decimal RefundedAmount
        {
            get
            {
                var refAmt = 0.00m;
                try
                {
                    if (Amount > 0) //we have a donation of some sort
                    {
                        if (Status == Enum_ChargeStatus.Refunded) //we need to ensure it's refunded... to get $ of refunds
                        {
                            //"<strong>Refunded Amount</strong>: $35.01<br />
                            const string token = "<strong>Refunded Amount</strong>: $";

                            var loc = Notes.IndexOf(token);

                            while (loc > 0)
                            {
                                var start = loc + token.Length;
                                var end = Notes.IndexOf("<br />", loc);

                                var refundAmt = Notes.Substring(start, end - start);
                                var localRefundAmt = 0.00m;
                                decimal.TryParse(refundAmt, out localRefundAmt);

                                refAmt += localRefundAmt;

                                loc = Notes.IndexOf(token, end);
                            }
                        }
                    }
                }
                catch (Exception)
                {}

                return refAmt;
            }
        }
    }
}
