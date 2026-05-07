/* eslint-disable */
/* tslint:disable */
// @ts-nocheck
/*
 * ---------------------------------------------------------------
 * ## THIS FILE WAS GENERATED VIA SWAGGER-TYPESCRIPT-API        ##
 * ##                                                           ##
 * ## AUTHOR: acacode                                           ##
 * ## SOURCE: https://github.com/acacode/swagger-typescript-api ##
 * ---------------------------------------------------------------
 */

export interface AdminSystemSettingsResponse {
  defaultStatusPageSlug: string;
}

export interface AdminUpdateSystemSettingsRequest {
  defaultStatusPageSlug?: string | null;
}

export interface CreateMonitorConfigRequest {
  name: string;
  type: string;
  target: string;
  enabled: boolean;
  /** @format int32 */
  interval: number;
  /** @format int32 */
  timeout: number;
  /** @format int32 */
  warningThreshold: number;
  settings: string;
}

export interface CreateStatusPageMonitorGroup {
  name: string;
  monitorIds: number[];
}

export interface CreateStatusPageRequest {
  /** @pattern ^[a-z0-9]+(?:-[a-z0-9]+)*$ */
  slug: string;
  name: string;
  monitorGroups: CreateStatusPageMonitorGroup[];
}

export interface CreateWorkerRequest {
  name: string;
  location: string;
  secret: string;
  /** @format int32 */
  configUpdateInterval: number;
}

export interface GetMonitorHeartbeatsResponse {
  /** @format int32 */
  monitorId: number;
  /** @format double */
  uptime: number;
  heartbeats: HeartbeatDto[];
}

export interface GetMonitorStatisticsResponse {
  /** @format int32 */
  totalMonitors: number;
  /** @format int32 */
  enabledMonitors: number;
  /** @format int32 */
  disabledMonitors: number;
  /** @format int32 */
  upMonitors: number;
  /** @format int32 */
  downMonitors: number;
}

export interface GetWorkerStatisticsResponse {
  /** @format int32 */
  totalWorkers: number;
  /** @format int32 */
  activeWorkers: number;
}

export interface HeartbeatDto {
  /** @format date-time */
  timestamp: string;
  /** @format double */
  responseTime: number;
  up?: boolean | null;
  /** @format int32 */
  totalHeartbeats: number;
  /** @format int32 */
  upHeartbeats: number;
}

export interface MonitorConfigurationResponse {
  /** @format int32 */
  id: number;
  name: string;
  type: string;
  target: string;
  enabled: boolean;
  /** @format int32 */
  interval: number;
  /** @format int32 */
  timeout: number;
  /** @format int32 */
  warningThreshold: number;
  settings: string;
  tlsDetails: MonitorTlsDetailsDto;
}

export interface MonitorEventDto {
  /** @format int32 */
  id: number;
  monitorName: string;
  monitorType: string;
  /** @format date-time */
  timestamp: string;
  eventType: string;
  eventMessage: string;
}

export interface MonitorEventDtoPagedListResponse {
  /** @format int32 */
  totalPages: number;
  /** @format int32 */
  totalRecords: number;
  records: MonitorEventDto[];
}

export interface MonitorSettingsPropertyDto {
  key: string;
  dataType: string;
  displayName: string;
  description: string;
  defaultValue?: any;
  required: boolean;
  allowMultiple: boolean;
}

export interface MonitorSettingsSchemaDto {
  monitorType: string;
  properties: MonitorSettingsPropertyDto[];
}

export interface MonitorSimpleDto {
  /** @format int32 */
  id: number;
  name: string;
  type: string;
  target: string;
  enabled: boolean;
}

export interface MonitorSimpleDtoPagedListResponse {
  /** @format int32 */
  totalPages: number;
  /** @format int32 */
  totalRecords: number;
  records: MonitorSimpleDto[];
}

export interface MonitorTlsDetailsDto {
  subject: string;
  issuer: string;
  /** @format date-time */
  notBefore: string;
  /** @format date-time */
  notAfter: string;
}

export interface MonitorTypeDto {
  key: string;
  displayName: string;
}

export interface StatusPageConfigResponse {
  /** @format int32 */
  id: number;
  slug: string;
  name: string;
  monitorGroups: StatusPageGroupConfigDto[];
}

export interface StatusPageGroupConfigDto {
  /** @format int32 */
  id: number;
  name: string;
  monitors: MonitorSimpleDto[];
}

export interface StatusPageSimpleDto {
  /** @format int32 */
  id: number;
  name: string;
  slug: string;
  default: boolean;
}

export interface StatusPageSimpleDtoPagedListResponse {
  /** @format int32 */
  totalPages: number;
  /** @format int32 */
  totalRecords: number;
  records: StatusPageSimpleDto[];
}

export interface UpdateMonitorConfigRequest {
  name: string;
  target: string;
  enabled: boolean;
  /** @format int32 */
  interval: number;
  /** @format int32 */
  timeout: number;
  /** @format int32 */
  warningThreshold: number;
  settings: string;
}

export interface UpdateMonitorGroupDto {
  /** @format int32 */
  id?: number | null;
  name: string;
  monitorIds: number[];
}

export interface UpdateStatusPageRequest {
  /** @pattern ^[a-z0-9]+(?:-[a-z0-9]+)*$ */
  slug: string;
  name: string;
  monitorGroups: UpdateMonitorGroupDto[];
}

export interface UpdateWorkerRequest {
  name: string;
  location: string;
  secret?: string | null;
  /** @format int32 */
  configUpdateInterval: number;
}

export interface WorkerConfigurationDto {
  /** @format int32 */
  id: number;
  name: string;
  location: string;
  /** @format int32 */
  configUpdateInterval: number;
  /** @format date-time */
  lastCheckIn: string;
}

export interface WorkerConfigurationDtoPagedListResponse {
  /** @format int32 */
  totalPages: number;
  /** @format int32 */
  totalRecords: number;
  records: WorkerConfigurationDto[];
}

export type QueryParamsType = Record<string | number, any>;
export type ResponseFormat = keyof Omit<Body, "body" | "bodyUsed">;

export interface FullRequestParams extends Omit<RequestInit, "body"> {
  /** set parameter to `true` for call `securityWorker` for this request */
  secure?: boolean;
  /** request path */
  path: string;
  /** content type of request body */
  type?: ContentType;
  /** query params */
  query?: QueryParamsType;
  /** format of response (i.e. response.json() -> format: "json") */
  format?: ResponseFormat;
  /** request body */
  body?: unknown;
  /** base url */
  baseUrl?: string;
  /** request cancellation token */
  cancelToken?: CancelToken;
}

export type RequestParams = Omit<
  FullRequestParams,
  "body" | "method" | "query" | "path"
>;

export interface ApiConfig<SecurityDataType = unknown> {
  baseUrl?: string;
  baseApiParams?: Omit<RequestParams, "baseUrl" | "cancelToken" | "signal">;
  securityWorker?: (
    securityData: SecurityDataType | null,
  ) => Promise<RequestParams | void> | RequestParams | void;
  customFetch?: typeof fetch;
}

export interface HttpResponse<D extends unknown, E extends unknown = unknown>
  extends Response {
  data: D;
  error: E;
}

type CancelToken = Symbol | string | number;

export enum ContentType {
  Json = "application/json",
  JsonApi = "application/vnd.api+json",
  FormData = "multipart/form-data",
  UrlEncoded = "application/x-www-form-urlencoded",
  Text = "text/plain",
}

export class HttpClient<SecurityDataType = unknown> {
  public baseUrl: string = "";
  private securityData: SecurityDataType | null = null;
  private securityWorker?: ApiConfig<SecurityDataType>["securityWorker"];
  private abortControllers = new Map<CancelToken, AbortController>();
  private customFetch = (...fetchParams: Parameters<typeof fetch>) =>
    fetch(...fetchParams);

  private baseApiParams: RequestParams = {
    credentials: "same-origin",
    headers: {},
    redirect: "follow",
    referrerPolicy: "no-referrer",
  };

  constructor(apiConfig: ApiConfig<SecurityDataType> = {}) {
    Object.assign(this, apiConfig);
  }

  public setSecurityData = (data: SecurityDataType | null) => {
    this.securityData = data;
  };

  protected encodeQueryParam(key: string, value: any) {
    const encodedKey = encodeURIComponent(key);
    return `${encodedKey}=${encodeURIComponent(typeof value === "number" ? value : `${value}`)}`;
  }

  protected addQueryParam(query: QueryParamsType, key: string) {
    return this.encodeQueryParam(key, query[key]);
  }

  protected addArrayQueryParam(query: QueryParamsType, key: string) {
    const value = query[key];
    return value.map((v: any) => this.encodeQueryParam(key, v)).join("&");
  }

  protected toQueryString(rawQuery?: QueryParamsType): string {
    const query = rawQuery || {};
    const keys = Object.keys(query).filter(
      (key) => "undefined" !== typeof query[key],
    );
    return keys
      .map((key) =>
        Array.isArray(query[key])
          ? this.addArrayQueryParam(query, key)
          : this.addQueryParam(query, key),
      )
      .join("&");
  }

  protected addQueryParams(rawQuery?: QueryParamsType): string {
    const queryString = this.toQueryString(rawQuery);
    return queryString ? `?${queryString}` : "";
  }

  private contentFormatters: Record<ContentType, (input: any) => any> = {
    [ContentType.Json]: (input: any) =>
      input !== null && (typeof input === "object" || typeof input === "string")
        ? JSON.stringify(input)
        : input,
    [ContentType.JsonApi]: (input: any) =>
      input !== null && (typeof input === "object" || typeof input === "string")
        ? JSON.stringify(input)
        : input,
    [ContentType.Text]: (input: any) =>
      input !== null && typeof input !== "string"
        ? JSON.stringify(input)
        : input,
    [ContentType.FormData]: (input: any) => {
      if (input instanceof FormData) {
        return input;
      }

      return Object.keys(input || {}).reduce((formData, key) => {
        const property = input[key];
        formData.append(
          key,
          property instanceof Blob
            ? property
            : typeof property === "object" && property !== null
              ? JSON.stringify(property)
              : `${property}`,
        );
        return formData;
      }, new FormData());
    },
    [ContentType.UrlEncoded]: (input: any) => this.toQueryString(input),
  };

  protected mergeRequestParams(
    params1: RequestParams,
    params2?: RequestParams,
  ): RequestParams {
    return {
      ...this.baseApiParams,
      ...params1,
      ...(params2 || {}),
      headers: {
        ...(this.baseApiParams.headers || {}),
        ...(params1.headers || {}),
        ...((params2 && params2.headers) || {}),
      },
    };
  }

  protected createAbortSignal = (
    cancelToken: CancelToken,
  ): AbortSignal | undefined => {
    if (this.abortControllers.has(cancelToken)) {
      const abortController = this.abortControllers.get(cancelToken);
      if (abortController) {
        return abortController.signal;
      }
      return void 0;
    }

    const abortController = new AbortController();
    this.abortControllers.set(cancelToken, abortController);
    return abortController.signal;
  };

  public abortRequest = (cancelToken: CancelToken) => {
    const abortController = this.abortControllers.get(cancelToken);

    if (abortController) {
      abortController.abort();
      this.abortControllers.delete(cancelToken);
    }
  };

  public request = async <T = any, E = any>({
    body,
    secure,
    path,
    type,
    query,
    format,
    baseUrl,
    cancelToken,
    ...params
  }: FullRequestParams): Promise<HttpResponse<T, E>> => {
    const secureParams =
      ((typeof secure === "boolean" ? secure : this.baseApiParams.secure) &&
        this.securityWorker &&
        (await this.securityWorker(this.securityData))) ||
      {};
    const requestParams = this.mergeRequestParams(params, secureParams);
    const queryString = query && this.toQueryString(query);
    const payloadFormatter = this.contentFormatters[type || ContentType.Json];
    const responseFormat = format || requestParams.format;

    return this.customFetch(
      `${baseUrl || this.baseUrl || ""}${path}${queryString ? `?${queryString}` : ""}`,
      {
        ...requestParams,
        headers: {
          ...(requestParams.headers || {}),
          ...(type && type !== ContentType.FormData
            ? { "Content-Type": type }
            : {}),
        },
        signal:
          (cancelToken
            ? this.createAbortSignal(cancelToken)
            : requestParams.signal) || null,
        body:
          typeof body === "undefined" || body === null
            ? null
            : payloadFormatter(body),
      },
    ).then(async (response) => {
      const r = response as HttpResponse<T, E>;
      r.data = null as unknown as T;
      r.error = null as unknown as E;

      const responseToParse = responseFormat ? response.clone() : response;
      const data = !responseFormat
        ? r
        : await responseToParse[responseFormat]()
            .then((data) => {
              if (r.ok) {
                r.data = data;
              } else {
                r.error = data;
              }
              return r;
            })
            .catch((e) => {
              r.error = e;
              return r;
            });

      if (cancelToken) {
        this.abortControllers.delete(cancelToken);
      }

      if (!response.ok) throw data;
      return data;
    });
  };
}

/**
 * @title UptimeMonitor Admin API
 * @version v1
 */
export class Api<
  SecurityDataType extends unknown,
> extends HttpClient<SecurityDataType> {
  admin = {
    /**
     * No description
     *
     * @tags Config
     * @name AdminGetSystemSettings
     * @request GET:/api/admin/config
     */
    adminGetSystemSettings: (params: RequestParams = {}) =>
      this.request<AdminSystemSettingsResponse, any>({
        path: `/api/admin/config`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Config
     * @name AdminUpdateSystemSettings
     * @request POST:/api/admin/config
     */
    adminUpdateSystemSettings: (
      data: AdminUpdateSystemSettingsRequest,
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/api/admin/config`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Monitor
     * @name AdminGetMonitorStatistics
     * @request GET:/api/admin/monitors/statistics
     */
    adminGetMonitorStatistics: (params: RequestParams = {}) =>
      this.request<GetMonitorStatisticsResponse, any>({
        path: `/api/admin/monitors/statistics`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Monitor
     * @name AdminGetMonitors
     * @request GET:/api/admin/monitors
     */
    adminGetMonitors: (
      query?: {
        /**
         * @format int32
         * @default 0
         */
        page?: number;
        /**
         * @format int32
         * @default 0
         */
        pageSize?: number;
        search?: string;
      },
      params: RequestParams = {},
    ) =>
      this.request<MonitorSimpleDtoPagedListResponse, any>({
        path: `/api/admin/monitors`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Monitor
     * @name AdminCreateMonitor
     * @request POST:/api/admin/monitors
     */
    adminCreateMonitor: (
      data: CreateMonitorConfigRequest,
      params: RequestParams = {},
    ) =>
      this.request<MonitorConfigurationResponse, any>({
        path: `/api/admin/monitors`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Monitor
     * @name AdminGetAllMonitorEvents
     * @request GET:/api/admin/monitors/events
     */
    adminGetAllMonitorEvents: (
      query?: {
        /**
         * @format int32
         * @default 0
         */
        page?: number;
        /**
         * @format int32
         * @default 0
         */
        pageSize?: number;
      },
      params: RequestParams = {},
    ) =>
      this.request<MonitorEventDtoPagedListResponse, any>({
        path: `/api/admin/monitors/events`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Monitor
     * @name AdminGetMonitorTypes
     * @request GET:/api/admin/monitors/types
     */
    adminGetMonitorTypes: (params: RequestParams = {}) =>
      this.request<MonitorTypeDto[], any>({
        path: `/api/admin/monitors/types`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Monitor
     * @name AdminGetMonitorTypeSettings
     * @request GET:/api/admin/monitors/type-settings
     */
    adminGetMonitorTypeSettings: (params: RequestParams = {}) =>
      this.request<MonitorSettingsSchemaDto[], any>({
        path: `/api/admin/monitors/type-settings`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Monitor
     * @name AdminGetMonitor
     * @request GET:/api/admin/monitors/{id}
     */
    adminGetMonitor: (id: number, params: RequestParams = {}) =>
      this.request<MonitorConfigurationResponse, any>({
        path: `/api/admin/monitors/${id}`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Monitor
     * @name AdminUpdateMonitor
     * @request PUT:/api/admin/monitors/{id}
     */
    adminUpdateMonitor: (
      id: number,
      data: UpdateMonitorConfigRequest,
      params: RequestParams = {},
    ) =>
      this.request<MonitorConfigurationResponse, any>({
        path: `/api/admin/monitors/${id}`,
        method: "PUT",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Monitor
     * @name AdminDeleteMonitor
     * @request DELETE:/api/admin/monitors/{id}
     */
    adminDeleteMonitor: (id: number, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/admin/monitors/${id}`,
        method: "DELETE",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Monitor
     * @name AdminGetMonitorEvents
     * @request GET:/api/admin/monitors/{id}/events
     */
    adminGetMonitorEvents: (
      id: number,
      query?: {
        /**
         * @format int32
         * @default 0
         */
        page?: number;
        /**
         * @format int32
         * @default 0
         */
        pageSize?: number;
      },
      params: RequestParams = {},
    ) =>
      this.request<MonitorEventDtoPagedListResponse, any>({
        path: `/api/admin/monitors/${id}/events`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Monitor
     * @name AdminGetChartDataForMonitor
     * @request GET:/api/admin/monitors/{id}/metrics
     */
    adminGetChartDataForMonitor: (id: number, params: RequestParams = {}) =>
      this.request<GetMonitorHeartbeatsResponse, any>({
        path: `/api/admin/monitors/${id}/metrics`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Monitor
     * @name AdminGetChartDataForMonitors
     * @request GET:/api/admin/monitors/metrics
     */
    adminGetChartDataForMonitors: (
      query?: {
        ids?: number[];
      },
      params: RequestParams = {},
    ) =>
      this.request<GetMonitorHeartbeatsResponse[], any>({
        path: `/api/admin/monitors/metrics`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Monitor
     * @name AdminGetChartDataAllMonitors
     * @request GET:/api/admin/monitors/metrics/all
     */
    adminGetChartDataAllMonitors: (params: RequestParams = {}) =>
      this.request<GetMonitorHeartbeatsResponse[], any>({
        path: `/api/admin/monitors/metrics/all`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags StatusPage
     * @name StatusPagesGetList
     * @request GET:/api/admin/status-pages
     */
    statusPagesGetList: (
      query?: {
        /**
         * @format int32
         * @default 0
         */
        page?: number;
        /**
         * @format int32
         * @default 0
         */
        pageSize?: number;
        search?: string;
      },
      params: RequestParams = {},
    ) =>
      this.request<StatusPageSimpleDtoPagedListResponse, any>({
        path: `/api/admin/status-pages`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags StatusPage
     * @name StatusPagesCreate
     * @request POST:/api/admin/status-pages
     */
    statusPagesCreate: (
      data: CreateStatusPageRequest,
      params: RequestParams = {},
    ) =>
      this.request<StatusPageConfigResponse, any>({
        path: `/api/admin/status-pages`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags StatusPage
     * @name StatusPagesGetBySlug
     * @request GET:/api/admin/status-pages/{slug}
     */
    statusPagesGetBySlug: (slug: string, params: RequestParams = {}) =>
      this.request<StatusPageConfigResponse, any>({
        path: `/api/admin/status-pages/${slug}`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags StatusPage
     * @name StatusPagesUpdate
     * @request PUT:/api/admin/status-pages/{slug}
     */
    statusPagesUpdate: (
      slug: string,
      data: UpdateStatusPageRequest,
      params: RequestParams = {},
    ) =>
      this.request<StatusPageConfigResponse, any>({
        path: `/api/admin/status-pages/${slug}`,
        method: "PUT",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags StatusPage
     * @name StatusPagesDelete
     * @request DELETE:/api/admin/status-pages/{slug}
     */
    statusPagesDelete: (slug: string, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/admin/status-pages/${slug}`,
        method: "DELETE",
        ...params,
      }),

    /**
     * No description
     *
     * @tags StatusPage
     * @name StatusPagesSetDefault
     * @request POST:/api/admin/status-pages/{slug}/set-default
     */
    statusPagesSetDefault: (slug: string, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/admin/status-pages/${slug}/set-default`,
        method: "POST",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Worker
     * @name GetWorkerStatistics
     * @request GET:/api/admin/workers/statistics
     */
    getWorkerStatistics: (params: RequestParams = {}) =>
      this.request<GetWorkerStatisticsResponse, any>({
        path: `/api/admin/workers/statistics`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Worker
     * @name GetWorkerList
     * @request GET:/api/admin/workers
     */
    getWorkerList: (
      query?: {
        /**
         * @format int32
         * @default 0
         */
        page?: number;
        /**
         * @format int32
         * @default 0
         */
        pageSize?: number;
        search?: string;
      },
      params: RequestParams = {},
    ) =>
      this.request<WorkerConfigurationDtoPagedListResponse, any>({
        path: `/api/admin/workers`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Worker
     * @name CreateWorker
     * @request POST:/api/admin/workers
     */
    createWorker: (data: CreateWorkerRequest, params: RequestParams = {}) =>
      this.request<WorkerConfigurationDto, any>({
        path: `/api/admin/workers`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Worker
     * @name GetWorkerById
     * @request GET:/api/admin/workers/{id}
     */
    getWorkerById: (id: number, params: RequestParams = {}) =>
      this.request<WorkerConfigurationDto, any>({
        path: `/api/admin/workers/${id}`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Worker
     * @name UpdateWorker
     * @request PUT:/api/admin/workers/{id}
     */
    updateWorker: (
      id: number,
      data: UpdateWorkerRequest,
      params: RequestParams = {},
    ) =>
      this.request<WorkerConfigurationDto, any>({
        path: `/api/admin/workers/${id}`,
        method: "PUT",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Worker
     * @name DeleteWorker
     * @request DELETE:/api/admin/workers/{id}
     */
    deleteWorker: (id: number, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/admin/workers/${id}`,
        method: "DELETE",
        ...params,
      }),
  };
}
