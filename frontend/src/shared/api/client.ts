import axios from 'axios';
import type { ApiErrorBody } from './types';

const baseURL = import.meta.env.VITE_API_URL || '';
const apiKey = import.meta.env.VITE_API_KEY || '';

export const apiClient = axios.create({
  baseURL,
  headers: {
    'Content-Type': 'application/json',
    'X-Api-Key': apiKey,
  },
});

export function getErrorMessage(error: unknown): string {
  if (axios.isAxiosError(error)) {
    const data = error.response?.data as ApiErrorBody | undefined;
    const fieldErrors = data?.errors ?? data?.Errors;
    if (fieldErrors) {
      return Object.values(fieldErrors).flat().join(' ');
    }
    const message = data?.message ?? data?.Message;
    if (message) {
      return message;
    }
    return error.message;
  }
  if (error instanceof Error) {
    return error.message;
  }
  return 'Unexpected error';
}
