using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _1131437_藍奕_房貸計算器
{
    public partial class MortgageCalculator : Form
    {
        public MortgageCalculator()
        {
            InitializeComponent();
        }

        private void MortgageCalculator_Load(object sender, EventArgs e)
        {
            // 讓程式一開啟時，預設選取「比例」
            rbPercent.Checked = true;
        }

        private void rbPercent_CheckedChanged(object sender, EventArgs e)
        {
            if (rbPercent.Checked)
            {
                // 嘗試取得「房屋總價」與目前的「金額」
                if (decimal.TryParse(txtHousePrice.Text, out decimal housePrice) &&
                    decimal.TryParse(txtDownPayment.Text, out decimal amount))
                {
                    // 必須防範房屋總價為 0 的情況，避免發生「除以零」的嚴重錯誤
                    if (housePrice > 0)
                    {
                        // 計算出比例 = (金額 / 房屋總價) * 100
                        decimal percent = (amount / housePrice) * 100m;

                        // 更新輸入框。比例通常最多保留兩位小數，"0.##" 代表若無小數則不顯示
                        txtDownPayment.Text = Math.Round(percent, 2).ToString("0.##");
                    }
                }
            }
        }

        private void rbAmount_CheckedChanged(object sender, EventArgs e)
        {
            // 必須判斷是否為 Checked 狀態。因為兩個 RadioButton 切換時，
            // 原本的會變成 unchecked，新的變成 checked，事件會觸發兩次。
            if (rbAmount.Checked)
            {
                // 嘗試取得「房屋總價」與目前的「比例」
                if (decimal.TryParse(txtHousePrice.Text, out decimal housePrice) &&
                    decimal.TryParse(txtDownPayment.Text, out decimal percent))
                {
                    // 計算出實際金額 = 房屋總價 * (比例 / 100)
                    decimal amount = housePrice * (percent / 100m);

                    // 更新輸入框。新台幣通常不留小數點，這裡用 "0" 格式化取整數
                    txtDownPayment.Text = Math.Round(amount).ToString("0");
                }
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            // 清空並重置輸入框
            txtHousePrice.Text = "";
            txtDownPayment.Text = "20";
            txtInterestRate.Text = "2.15";
            txtLoanYears.Text = "30";
            txtGracePeriod.Text = "0";
            rbPercent.Checked = true;

            // 重置輸出結果
            lblTotalLoan.Text = "$ 0.00";
            lblMonthlyPayment.Text = "$ 0.00";
            lblFirstMonthInterest.Text = "$ 0.00";
            lblFirstMonthPrincipal.Text = "$ 0.00";
            lblTotalInterest.Text = "$ 0.00";
            lblTotalRepayment.Text = "$ 0.00";
        }

        private void btnCalculate_Click(object sender, EventArgs e)
        {
            // ==========================================
            // 1. 防呆驗證與讀取輸入 (有效攔截非法輸入 )
                        // ==========================================
            if (!decimal.TryParse(txtHousePrice.Text, out decimal housePrice) || housePrice <= 0)
            {
                MessageBox.Show("請輸入大於 0 的房屋總價！", "輸入錯誤", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!decimal.TryParse(txtDownPayment.Text, out decimal downPayment) || downPayment < 0)
            {
                MessageBox.Show("請輸入正確的自備款！", "輸入錯誤", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!decimal.TryParse(txtInterestRate.Text, out decimal annualRate) || annualRate < 0)
            {
                MessageBox.Show("請輸入正確的貸款利率！", "輸入錯誤", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!int.TryParse(txtLoanYears.Text, out int loanYears) || loanYears <= 0)
            {
                MessageBox.Show("請輸入正確的貸款年限！", "輸入錯誤", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // 寬限期為選填，若未填或填錯，預設為 0 [cite: 16]
            if (!int.TryParse(txtGracePeriod.Text, out int graceYears) || graceYears < 0)
            {
                graceYears = 0;
            }

            // ==========================================
            // 2. 核心公式計算 
            // ==========================================
            // 計算貸款總金額 [cite: 20]
            decimal downPaymentAmount = rbPercent.Checked ? housePrice * (downPayment / 100m) : downPayment;
            decimal totalLoanAmount = housePrice - downPaymentAmount;

            if (totalLoanAmount <= 0)
            {
                MessageBox.Show("自備款已大於或等於房屋總價，無需貸款！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            double P = (double)totalLoanAmount;
            double r = (double)(annualRate / 100m / 12m); // 月利率
            int n = loanYears * 12; // 總月數
            int n_g = graceYears * 12; // 寬限期月數

            double monthlyPayment = 0;
            decimal firstMonthInterest = (decimal)(P * r);
            decimal firstMonthPrincipal = 0;
            decimal totalRepayment = 0;

            if (graceYears > 0)
            {
                // 有寬限期的情況
                int n_r = n - n_g; // 剩餘攤還月數
                monthlyPayment = P * (r * Math.Pow(1 + r, n_r)) / (Math.Pow(1 + r, n_r) - 1);

                firstMonthPrincipal = 0; // 寬限期首期不繳本金 [cite: 22]
                totalRepayment = (decimal)(n_g * (P * r)) + (decimal)((n - n_g) * monthlyPayment); // 總還款金額 [cite: 24]
            }
            else
            {
                // 無寬限期的情況
                monthlyPayment = P * (r * Math.Pow(1 + r, n)) / (Math.Pow(1 + r, n) - 1);

                firstMonthPrincipal = (decimal)monthlyPayment - firstMonthInterest; // 首期本金 [cite: 22]
                totalRepayment = (decimal)monthlyPayment * n; // 總還款金額 [cite: 24]
            }

            decimal totalInterest = totalRepayment - totalLoanAmount; // 總利息支出 [cite: 23]

            // ==========================================
            // 3. 顯示結果並格式化 (含千分位逗號與小數點後兩位 [cite: 19])
                        // ==========================================
            lblTotalLoan.Text = $"$ {totalLoanAmount:N2}";

            // 若有寬限期，這裡顯示的是「寬限期後」的每月本息攤還金額 [cite: 21]
            lblMonthlyPayment.Text = $"$ {monthlyPayment:N2}";

            lblFirstMonthInterest.Text = $"$ {firstMonthInterest:N2}";
            lblFirstMonthPrincipal.Text = $"$ {firstMonthPrincipal:N2}";
            lblTotalInterest.Text = $"$ {totalInterest:N2}";
            lblTotalRepayment.Text = $"$ {totalRepayment:N2}";
        }
    }

}
