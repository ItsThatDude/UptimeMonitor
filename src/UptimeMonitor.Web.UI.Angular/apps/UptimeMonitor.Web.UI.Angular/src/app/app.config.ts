import { ApplicationConfig, inject, provideAppInitializer, provideZoneChangeDetection } from '@angular/core';
import { provideRouter, withComponentInputBinding } from '@angular/router';
import { appRoutes } from './app.routes';
import { provideAdminApiClient } from './providers/admin-api.provider';
import { providePublicApiClient } from './providers/public-api.provider';
import { provideHttpClient } from '@angular/common/http';
import { provideCharts } from 'ng2-charts';
import { BarController, BarElement, Colors, Legend, LinearScale, TimeScale, TimeSeriesScale, Tooltip } from 'chart.js';
import Annotation from 'chartjs-plugin-annotation';
import 'chartjs-adapter-date-fns';
import { ConfigService } from './services/config.service';

export function initializeApp() {
  const configService = inject(ConfigService);
  configService.loadAppConfig();
}

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(appRoutes, withComponentInputBinding()),
    provideHttpClient(),
    provideAppInitializer(initializeApp),
    provideAdminApiClient(),
    providePublicApiClient(),
    provideCharts({ registerables: [BarController, BarElement, TimeScale, TimeSeriesScale, LinearScale, Legend, Colors, Tooltip, Annotation] })
  ],
};
