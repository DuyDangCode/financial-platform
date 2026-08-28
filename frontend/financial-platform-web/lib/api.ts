/**
 * Minimal dependency-free API client for the Financial Platform backend.
 *
 * Every endpoint returns the `ApiResponse<T>` envelope:
 *   { success, message?, data?, error? }
 * where `error` is `{ statusCode, message, validationErrors? }`.
 *
 * Base URL: NEXT_PUBLIC_API_URL if set, else the dev API port.
 */

export const API_BASE_URL =
  process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5150";

export interface ApiValidationError {
  field: string;
  message: string;
}

export interface ApiErrorPayload {
  statusCode: number;
  message: string;
  validationErrors?: ApiValidationError[] | null;
}

export interface ApiResponseEnvelope<T> {
  success: boolean;
  message?: string | null;
  data?: T;
  error?: ApiErrorPayload | null;
}

/** Thrown when a request fails (network error, non-success envelope, or HTTP error). */
export class ApiRequestError extends Error {
  readonly statusCode: number;
  readonly validationErrors: ApiValidationError[];

  constructor(
    message: string,
    options: {
      statusCode?: number;
      validationErrors?: ApiValidationError[];
      cause?: unknown;
    } = {},
  ) {
    super(message, { cause: options.cause });
    this.name = "ApiRequestError";
    this.statusCode = options.statusCode ?? 0;
    this.validationErrors = options.validationErrors ?? [];
  }
}

const NETWORK_ERROR_MESSAGE = "Unable to reach the server. Please try again.";

async function request<T>(path: string, body: unknown): Promise<T> {
  let response: Response;

  try {
    response = await fetch(`${API_BASE_URL}${path}`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
    });
  } catch (cause) {
    // fetch rejects on network failures / untrusted dev certificates.
    throw new ApiRequestError(NETWORK_ERROR_MESSAGE, { cause });
  }

  let envelope: ApiResponseEnvelope<T> | null = null;
  try {
    envelope = (await response.json()) as ApiResponseEnvelope<T>;
  } catch {
    // Fall through: non-JSON body is handled below.
  }

  if (!response.ok || !envelope || !envelope.success) {
    const error = envelope?.error;
    throw new ApiRequestError(error?.message ?? NETWORK_ERROR_MESSAGE, {
      statusCode: error?.statusCode ?? response.status,
      validationErrors: error?.validationErrors ?? [],
    });
  }

  if (envelope.data === undefined || envelope.data === null) {
    throw new ApiRequestError("The server returned an unexpected response.", {
      statusCode: response.status,
    });
  }

  return envelope.data;
}

/** POST a JSON body and unwrap the `ApiResponse<T>` envelope. */
export function postJson<T>(path: string, body: unknown): Promise<T> {
  return request<T>(path, body);
}
