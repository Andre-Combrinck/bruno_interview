export function formatMoney(amount: number, currency = 'R'): string {
  return `${currency} ${amount.toFixed(2)}`;
}
