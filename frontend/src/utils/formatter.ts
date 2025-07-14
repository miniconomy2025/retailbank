export function formatCurrency(value: number) {
  return value >= 0
    ? `Đ ${formatMoney(value)} Dr`
    : `Đ ${formatMoney(-value)} Cr`;
}

export function formatMoney(value: number) {
  return (
    (value/100)
      .toFixed(2)
      .replace(/\B(?=(\d{3})+(?!\d))/g, " ")
  );
}
