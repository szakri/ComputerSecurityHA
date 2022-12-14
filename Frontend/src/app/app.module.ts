import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FlexLayoutModule } from '@angular/flex-layout';
import { MatIconModule } from '@angular/material/icon';
import { MatDialogModule } from '@angular/material/dialog';
import { MatSnackBarModule } from '@angular/material/snack-bar'; 




import { AppComponent } from './app.component';
import { AppRoutingModule } from './app-routing.module';
import { DialogUploadCAFF, MainComponent } from './main/main.component';
import { DetailComponent, DialogConfirmDelete, DialogEditCAFF } from './detail/detail.component';
import { AuthComponent } from './auth/auth.component';
import { HeaderComponent } from './header/header.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule, } from '@angular/material/form-field';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatGridListModule } from '@angular/material/grid-list';
import { AuthInterceptor, AuthService } from './services/authservice';


@NgModule({
  declarations: [
    AppComponent,
    MainComponent,
    DetailComponent,
    AuthComponent,
    HeaderComponent,
    DialogEditCAFF,
    DialogUploadCAFF,
    DialogConfirmDelete
  ],
  imports: [
    BrowserModule, HttpClientModule, AppRoutingModule, BrowserAnimationsModule,
    MatToolbarModule,
    MatCardModule,
    MatButtonModule,
    MatInputModule,
    MatFormFieldModule,
    ReactiveFormsModule,
    MatTooltipModule,
    MatGridListModule,
    FlexLayoutModule,
    MatIconModule,
    MatDialogModule,
    FormsModule,
    MatSnackBarModule
  ],
  providers: [
    AuthService,
    { provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true }
  ],
  bootstrap: [AppComponent],
  entryComponents: [DialogEditCAFF, DialogUploadCAFF, DialogConfirmDelete]
})
export class AppModule { }
