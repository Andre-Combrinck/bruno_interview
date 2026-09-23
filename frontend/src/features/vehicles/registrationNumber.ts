const PROVINCE_SUFFIXES = ['KZN', 'EC', 'FS', 'GP', 'LP', 'MP', 'NC', 'NW', 'WC', 'WP'] as const;

export const REGISTRATION_MAX_LENGTH = 12;
export const REGISTRATION_INVALID_MESSAGE = 'Registration number must be a valid South African plate.';

/** Uppercase and strip spaces/hyphens — mirrors backend RegistrationNumber.Normalize. */
export function normalizeRegistrationNumber(value: string): string {
  return value.trim().replace(/[\s-]/g, '').toUpperCase();
}

function splitProvince(normalized: string): string | null {
  for (const suffix of PROVINCE_SUFFIXES) {
    if (normalized.length > suffix.length && normalized.endsWith(suffix)) {
      return normalized.slice(0, -suffix.length);
    }
  }
  return null;
}

/**
 * Broad SA plate check: legacy provincial, newer provincial + suffix, personalised + suffix.
 * Mirrors Bruno.Domain.ValueObjects.RegistrationNumber.IsValid.
 */
export function isValidSouthAfricanRegistration(value: string): boolean {
  const normalized = normalizeRegistrationNumber(value);

  if (normalized.length < 3 || normalized.length > REGISTRATION_MAX_LENGTH) {
    return false;
  }

  if (!/^[A-Z0-9]+$/.test(normalized)) {
    return false;
  }

  if (/^[A-Z]{1,3}\d{1,6}$/.test(normalized)) {
    return true;
  }

  const body = splitProvince(normalized);
  if (!body || body.length < 1 || body.length > 8 || !/[A-Z]/.test(body)) {
    return false;
  }

  // Newer provincial: 2–8 alphanumeric with at least one digit.
  if (body.length >= 2 && body.length <= 8 && /\d/.test(body)) {
    return true;
  }

  // Personalised: 1–7 alphanumeric with at least one letter.
  return body.length <= 7;
}
