import { Component } from '@angular/core';

@Component({
  selector: 'app-main-layout',
  templateUrl: './main-layout.component.html',
  styleUrls: ['./main-layout.component.scss'],
  standalone: false
})
export class MainLayoutComponent {
  isSidebarCollapsed = false;
  isMobileSidebarOpen = false;

  onSidebarCollapse(collapsed: boolean): void {
    this.isSidebarCollapsed = collapsed;
  }

  onMenuToggle(): void {
    this.isMobileSidebarOpen = !this.isMobileSidebarOpen;
  }
}