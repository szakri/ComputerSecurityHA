import { Component, Inject, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { Caff } from '../models/caff';
import { CaffService } from '../services/caffservice';

@Component({
  selector: 'app-main',
  templateUrl: './main.component.html',
  styleUrls: ['./main.component.css']
})
export class MainComponent implements OnInit {
  searchFormControl = new FormControl('');
  caffs: Caff[] = [];

  constructor(private caffService: CaffService, private dialog: MatDialog) { }

  filterCAFFs() {
    this.getCaffsByName(this.searchFormControl.value);
  }
  ngOnInit(): void {
    this.getCaffs();
  }
  getCaffs() {
    this.caffService.getCaffs(null).subscribe(res => {
      this.caffs = res;
    })
  }
  getCaffsByName(searchterm: string | null) {
    if (searchterm != null) {
      this.caffService.getCaffs(searchterm).subscribe(res => {
        this.caffs = res;
      })
    }
  }

  getPreview(id: string) {
    return this.caffService.getCaffPreview(id);
  }

  getPreviewGif(id:string) {
    let url = "https://localhost:7235/api/caffs/" + id + "/preview";;
    return url;
  }

  clearFormControl() {
    this.searchFormControl.setValue("");
    this.getCaffs();
  }

  openUploadDialog() {
    const dialogRef = this.dialog.open(DialogUploadCAFF);
    dialogRef.afterClosed().subscribe(result => {
      console.log('The dialog was closed');
      if (result != null) {
        this.getCaffs();
      }
    });
  }

}

@Component({
  selector: 'dialog-data-example-dialog',
  templateUrl: 'dialog-upload.html',
})
export class DialogUploadCAFF implements OnInit {
  fileName = '';
  formData = new FormData();
  file!: File;


  constructor(public dialogRef: MatDialogRef<DialogUploadCAFF>, private caffService: CaffService) { }

  ngOnInit() {
    
  }

  onFileSelected(event: Event) {
    const target = event.target as HTMLInputElement;
    this.file = target.files![0];

    /*if (this.file) {

      this.fileName = file.name;

      this.formData.append("thumbnail", file);
      this.formData;
      
    }*/
  }

  cancelDialog() {
  }

  saveDialog() {
    const upload$ = this.caffService.uploadCaff(this.file);

    upload$.subscribe(res => 
      this.dialogRef.close()
    );
  }
}
