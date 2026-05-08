// src/app/shared/components/holiday-banner/holiday-banner.component.ts
import {
  Component, Input, ChangeDetectionStrategy
} from '@angular/core';
import { HolidayStatusDto } from '../../../core/models/HolidayStatusDto';

@Component({
  selector: 'app-holiday-banner',
  templateUrl: './holiday-banner.component.html',
  styleUrls: ['./holiday-banner.component.scss'],
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class HolidayBannerComponent {
  @Input() status!: HolidayStatusDto;
  dismissed = false;

  dismiss(): void {
    this.dismissed = true;
  }
}