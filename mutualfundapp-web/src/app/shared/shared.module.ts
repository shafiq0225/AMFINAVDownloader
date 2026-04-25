import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { SidebarComponent } from './components/sidebar/sidebar.component';
import { TopbarComponent } from './components/topbar/topbar.component';
import { StatCardComponent } from './components/stat-card/stat-card.component';
import { LoadingSpinnerComponent } from './components/loading-spinner/loading-spinner.component';
import { NavChartComponent } from './components/nav-chart/nav-chart.component';

@NgModule({
    declarations: [
        SidebarComponent,
        TopbarComponent,
        StatCardComponent,
        LoadingSpinnerComponent,
        NavChartComponent
    ],
    imports: [
        CommonModule,
        RouterModule,
        FormsModule,
        ReactiveFormsModule
    ],
    exports: [
        // Angular modules re-exported for convenience
        CommonModule,
        RouterModule,
        FormsModule,
        ReactiveFormsModule,
        // Components
        SidebarComponent,
        TopbarComponent,
        StatCardComponent,
        LoadingSpinnerComponent,
        NavChartComponent
    ]
})
export class SharedModule { }