import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { SidebarComponent } from './components/sidebar/sidebar.component';
import { TopbarComponent } from './components/topbar/topbar.component';
import { StatCardComponent } from './components/stat-card/stat-card.component';
import { LoadingSpinnerComponent } from './components/loading-spinner/loading-spinner.component';
import { NavChartComponent } from './components/nav-chart/nav-chart.component';
import { NavGrowCountPipe } from './pipes/nav-grow-count.pipe';

@NgModule({
    declarations: [
        SidebarComponent,
        TopbarComponent,
        StatCardComponent,
        LoadingSpinnerComponent,
        NavChartComponent,
        NavGrowCountPipe 
    ],
    imports: [
        CommonModule,
        RouterModule,
        FormsModule,
        ReactiveFormsModule
    ],
    exports: [
        CommonModule,
        RouterModule,
        FormsModule,
        ReactiveFormsModule,
        SidebarComponent,
        TopbarComponent,
        StatCardComponent,
        LoadingSpinnerComponent,
        NavChartComponent,
        NavGrowCountPipe
    ]
})
export class SharedModule { }