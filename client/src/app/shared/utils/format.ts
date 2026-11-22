/** Formats a (possibly huge) integer string with thousands separators. */
export function formatBig(value: string | bigint): string {
  const v = typeof value === 'bigint' ? value : BigInt(value || '0');
  return v.toLocaleString('en-US');
}
