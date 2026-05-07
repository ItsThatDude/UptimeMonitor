{{/*
Expand the name of the chart.
*/}}
{{- define "uptime-monitor.name" -}}
{{- default .Chart.Name .Values.web.nameOverride | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Create a default fully qualified app name.
We truncate at 63 chars because some Kubernetes name fields are limited to this (by the DNS naming spec).
If release name contains chart name it will be used as a full name.
*/}}
{{- define "uptime-monitor.fullname" -}}
{{- if .Values.fullnameOverride }}
{{- .Values.fullnameOverride | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- $name := default .Chart.Name .Values.nameOverride }}
{{- if contains $name .Release.Name }}
{{- .Release.Name | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- printf "%s-%s" .Release.Name $name | trunc 63 | trimSuffix "-" }}
{{- end }}
{{- end }}
{{- end }}

{{/*
Create chart name and version as used by the chart label.
*/}}
{{- define "uptime-monitor.chart" -}}
{{- printf "%s-%s" .Chart.Name .Chart.Version | replace "+" "_" | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Common labels
*/}}
{{- define "uptime-monitor.labels" -}}
helm.sh/chart: {{ include "uptime-monitor.chart" . }}
{{ include "uptime-monitor.common.selectorLabels" . }}
{{- if .Chart.AppVersion }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
{{- end }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end }}

{{- define "uptime-monitor.web.labels" -}}
{{ include "uptime-monitor.labels" . }}
app.kubernetes.io/component: web
{{- end }}

{{- define "uptime-monitor.monitor.labels" -}}
{{ include "uptime-monitor.labels" . }}
app.kubernetes.io/component: monitor
{{- end }}

{{/*
Selector labels
*/}}
{{- define "uptime-monitor.common.selectorLabels" -}}
app.kubernetes.io/name: {{ include "uptime-monitor.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end }}

{{- define "uptime-monitor.web.selectorLabels" -}}
{{ include "uptime-monitor.common.selectorLabels" . }}
app.kubernetes.io/component: web
{{- end }}

{{- define "uptime-monitor.monitor.selectorLabels" -}}
{{ include "uptime-monitor.common.selectorLabels" . }}
app.kubernetes.io/component: monitor
{{- end }}

{{/*
Create the name of the service account to use for the web pods
*/}}
{{- define "uptime-monitor.web.serviceAccountName" -}}
{{- if .Values.web.serviceAccount.create }}
{{- default (printf "%s-web" (include "uptime-monitor.fullname" .)) .Values.web.serviceAccount.name }}
{{- else }}
{{- default "default" .Values.web.serviceAccount.name }}
{{- end }}
{{- end }}

{{/*
Create the name of the service account to use for the monitor pods
*/}}
{{- define "uptime-monitor.monitor.serviceAccountName" -}}
{{- if .Values.monitor.serviceAccount.create }}
{{- default (printf "%s-monitor" (include "uptime-monitor.fullname" .)) .Values.monitor.serviceAccount.name }}
{{- else }}
{{- default "default" .Values.monitor.serviceAccount.name }}
{{- end }}
{{- end }}
