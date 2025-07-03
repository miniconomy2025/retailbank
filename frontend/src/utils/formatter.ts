export function formatCurrency(value: number | undefined) {
  const val = value ?? 0;

  return (
    "Đ " +
    val
      .toFixed(2)
      .replace(/\B(?=(\d{3})+(?!\d))/g, " ")
  );
}
