// Connection-state store
export const transportState = $state({
  status: 'closed',          // 'closed' | 'connecting' | 'open' | 'error'
  lastError: '',
  isMock: false,
});

export function setStatus(s) { transportState.status = s; }
export function setError(msg) {
  transportState.lastError = msg || '';
  transportState.status = 'error';
}
