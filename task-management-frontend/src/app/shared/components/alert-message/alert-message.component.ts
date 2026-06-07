import { Component, Input, Output, EventEmitter, OnInit, OnChanges, SimpleChanges } from '@angular/core';

export type AlertType = 'success' | 'error' | 'warning';

@Component({
  selector: 'app-alert-message',
  templateUrl: './alert-message.component.html',
  styleUrls: ['./alert-message.component.scss']
})
export class AlertMessageComponent implements OnInit, OnChanges {
  @Input() type: AlertType = 'error';
  @Input() message: string | null = null;
  @Input() autoClose: boolean = false;
  @Input() duration: number = 5000;

  @Output() close = new EventEmitter<void>();

  private timeoutId: any = null;

  ngOnInit(): void {
    this.initAutoClose();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['message'] && !changes['message'].isFirstChange()) {
      this.initAutoClose();
    }
  }

  private initAutoClose(): void {
    if (this.timeoutId) {
      clearTimeout(this.timeoutId);
    }

    if (this.autoClose && this.message) {
      this.timeoutId = setTimeout(() => {
        this.onClose();
      }, this.duration);
    }
  }

  onClose(): void {
    if (this.timeoutId) {
      clearTimeout(this.timeoutId);
    }
    this.message = null;
    this.close.emit();
  }
}