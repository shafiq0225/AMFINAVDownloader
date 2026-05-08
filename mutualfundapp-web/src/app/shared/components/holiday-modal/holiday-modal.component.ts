// src/app/shared/components/holiday-modal/holiday-modal.component.ts
import {
  Component, Input, Output, EventEmitter,
  ChangeDetectionStrategy
} from '@angular/core';
import { HolidayStatusDto } from '../../../core/models/HolidayStatusDto';

@Component({
  selector: 'app-holiday-modal',
  templateUrl: './holiday-modal.component.html',
  styleUrls: ['./holiday-modal.component.scss'],
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class HolidayModalComponent {
  @Input() status!: HolidayStatusDto;
  @Output() dismissed = new EventEmitter<void>();

  onContinue(): void {
    this.dismissed.emit();
  }
}