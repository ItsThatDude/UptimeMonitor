{{/*
Expand the name of the chart.
*/}}
{{- define "uptime-monitor.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" }}
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

{{- define "uptime-monitor.server.labels" -}}
{{ include "uptime-monitor.labels" . }}
app.kubernetes.io/component: server
{{- end }}

{{- define "uptime-monitor.remoteMonitor.labels" -}}
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

{{- define "uptime-monitor.server.selectorLabels" -}}
{{ include "uptime-monitor.common.selectorLabels" . }}
app.kubernetes.io/component: server
{{- end }}

{{- define "uptime-monitor.remoteMonitor.selectorLabels" -}}
{{ include "uptime-monitor.common.selectorLabels" . }}
app.kubernetes.io/component: monitor
{{- end }}

{{/*
Create the name of the service account to use for the server pods
*/}}
{{- define "uptime-monitor.server.serviceAccountName" -}}
{{- if .Values.server.serviceAccount.create }}
{{- default (printf "%s-server" (include "uptime-monitor.fullname" .)) .Values.server.serviceAccount.name }}
{{- else }}
{{- default "default" .Values.server.serviceAccount.name }}
{{- end }}
{{- end }}

{{/*
Create the name of the service account to use for the monitor pods
*/}}
{{- define "uptime-monitor.remoteMonitor.serviceAccountName" -}}
{{- if .Values.remoteMonitor.serviceAccount.create }}
{{- default (printf "%s-monitor" (include "uptime-monitor.fullname" .)) .Values.remoteMonitor.serviceAccount.name }}
{{- else }}
{{- default "default" .Values.remoteMonitor.serviceAccount.name }}
{{- end }}
{{- end }}
