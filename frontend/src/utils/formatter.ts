export function formatCurrency(value: number) {
  return (
    "Đ " +
    (value/100)
      .toFixed(2)
      .replace(/\B(?=(\d{3})+(?!\d))/g, " ")
  );
}
